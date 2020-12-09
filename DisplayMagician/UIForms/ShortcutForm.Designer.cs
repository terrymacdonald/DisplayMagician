using DisplayMagician.Resources;
using DisplayMagician.Shared.UserControls;

namespace DisplayMagician.UIForms
{
    partial class ShortcutForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutForm));
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.il_games = new System.Windows.Forms.ImageList(this.components);
            this.dialog_open = new System.Windows.Forms.OpenFileDialog();
            this.tabc_shortcut = new System.Windows.Forms.TabControl();
            this.tabp_display = new System.Windows.Forms.TabPage();
            this.lbl_profile_shown_subtitle = new System.Windows.Forms.Label();
            this.lbl_profile_shown = new System.Windows.Forms.Label();
            this.ilv_saved_profiles = new Manina.Windows.Forms.ImageListView();
            this.dv_profile = new DisplayMagician.Shared.UserControls.DisplayView();
            this.tabp_audio = new System.Windows.Forms.TabPage();
            this.gb_audio_volume = new System.Windows.Forms.GroupBox();
            this.rb_set_audio_volume = new System.Windows.Forms.RadioButton();
            this.rb_keep_audio_volume = new System.Windows.Forms.RadioButton();
            this.lbl_audio_volume = new System.Windows.Forms.Label();
            this.nud_audio_volume = new System.Windows.Forms.NumericUpDown();
            this.btn_rescan_audio = new System.Windows.Forms.Button();
            this.cb_audio_device = new System.Windows.Forms.ComboBox();
            this.rb_change_audio = new System.Windows.Forms.RadioButton();
            this.rb_no_change_audio = new System.Windows.Forms.RadioButton();
            this.tabp_before = new System.Windows.Forms.TabPage();
            this.pnl_start_program4 = new System.Windows.Forms.Panel();
            this.cb_start_program4 = new System.Windows.Forms.CheckBox();
            this.txt_start_program4 = new System.Windows.Forms.TextBox();
            this.cb_start_program_close4 = new System.Windows.Forms.CheckBox();
            this.btn_start_program4 = new System.Windows.Forms.Button();
            this.txt_start_program_args4 = new System.Windows.Forms.TextBox();
            this.cb_start_program_pass_args4 = new System.Windows.Forms.CheckBox();
            this.lbl_start_program4 = new System.Windows.Forms.Label();
            this.pnl_start_program3 = new System.Windows.Forms.Panel();
            this.cb_start_program3 = new System.Windows.Forms.CheckBox();
            this.txt_start_program3 = new System.Windows.Forms.TextBox();
            this.cb_start_program_close3 = new System.Windows.Forms.CheckBox();
            this.btn_start_program3 = new System.Windows.Forms.Button();
            this.txt_start_program_args3 = new System.Windows.Forms.TextBox();
            this.cb_start_program_pass_args3 = new System.Windows.Forms.CheckBox();
            this.lbl_start_program3 = new System.Windows.Forms.Label();
            this.pnl_start_program2 = new System.Windows.Forms.Panel();
            this.cb_start_program2 = new System.Windows.Forms.CheckBox();
            this.txt_start_program2 = new System.Windows.Forms.TextBox();
            this.cb_start_program_close2 = new System.Windows.Forms.CheckBox();
            this.btn_start_program2 = new System.Windows.Forms.Button();
            this.txt_start_program_args2 = new System.Windows.Forms.TextBox();
            this.cb_start_program_pass_args2 = new System.Windows.Forms.CheckBox();
            this.lbl_start_program2 = new System.Windows.Forms.Label();
            this.pnl_start_program1 = new System.Windows.Forms.Panel();
            this.cb_start_program1 = new System.Windows.Forms.CheckBox();
            this.txt_start_program1 = new System.Windows.Forms.TextBox();
            this.cb_start_program_close1 = new System.Windows.Forms.CheckBox();
            this.btn_start_program1 = new System.Windows.Forms.Button();
            this.txt_start_program_args1 = new System.Windows.Forms.TextBox();
            this.cb_start_program_pass_args1 = new System.Windows.Forms.CheckBox();
            this.lbl_start_program1 = new System.Windows.Forms.Label();
            this.tabp_game = new System.Windows.Forms.TabPage();
            this.p_standalone = new System.Windows.Forms.Panel();
            this.btn_exe_to_start = new System.Windows.Forms.Button();
            this.txt_args_executable = new System.Windows.Forms.TextBox();
            this.cb_args_executable = new System.Windows.Forms.CheckBox();
            this.btn_app_different_executable = new System.Windows.Forms.Button();
            this.txt_alternative_executable = new System.Windows.Forms.TextBox();
            this.rb_wait_alternative_executable = new System.Windows.Forms.RadioButton();
            this.rb_wait_executable = new System.Windows.Forms.RadioButton();
            this.txt_executable = new System.Windows.Forms.TextBox();
            this.lbl_app_executable = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nud_timeout_executable = new System.Windows.Forms.NumericUpDown();
            this.rb_standalone = new System.Windows.Forms.RadioButton();
            this.rb_no_game = new System.Windows.Forms.RadioButton();
            this.p_game = new System.Windows.Forms.Panel();
            this.txt_game_launcher = new System.Windows.Forms.TextBox();
            this.txt_game_name = new System.Windows.Forms.TextBox();
            this.lbl_game_library = new System.Windows.Forms.Label();
            this.lbl_game_name = new System.Windows.Forms.Label();
            this.btn_choose_game = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_args_game = new System.Windows.Forms.TextBox();
            this.cb_args_game = new System.Windows.Forms.CheckBox();
            this.lbl_game_timeout = new System.Windows.Forms.Label();
            this.nud_timeout_game = new System.Windows.Forms.NumericUpDown();
            this.lv_games = new System.Windows.Forms.ListView();
            this.clm_images = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clm_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rb_launcher = new System.Windows.Forms.RadioButton();
            this.tabp_after = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_switch_audio_permanent = new System.Windows.Forms.RadioButton();
            this.rb_switch_audio_temp = new System.Windows.Forms.RadioButton();
            this.gb_display_after = new System.Windows.Forms.GroupBox();
            this.rb_switch_display_permanent = new System.Windows.Forms.RadioButton();
            this.rb_switch_display_temp = new System.Windows.Forms.RadioButton();
            this.txt_shortcut_save_name = new System.Windows.Forms.TextBox();
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_shortcut_name = new System.Windows.Forms.Label();
            this.cb_autosuggest = new System.Windows.Forms.CheckBox();
            this.tabc_shortcut.SuspendLayout();
            this.tabp_display.SuspendLayout();
            this.tabp_audio.SuspendLayout();
            this.gb_audio_volume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_audio_volume)).BeginInit();
            this.tabp_before.SuspendLayout();
            this.pnl_start_program4.SuspendLayout();
            this.pnl_start_program3.SuspendLayout();
            this.pnl_start_program2.SuspendLayout();
            this.pnl_start_program1.SuspendLayout();
            this.tabp_game.SuspendLayout();
            this.p_standalone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_executable)).BeginInit();
            this.p_game.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_game)).BeginInit();
            this.tabp_after.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gb_display_after.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Enabled = false;
            this.btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(517, 754);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(120, 40);
            this.btn_save.TabIndex = 6;
            this.btn_save.Text = "&Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_cancel.ForeColor = System.Drawing.Color.White;
            this.btn_cancel.Location = new System.Drawing.Point(1008, 769);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(94, 25);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "&Back";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // il_games
            // 
            this.il_games.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.il_games.ImageSize = new System.Drawing.Size(32, 32);
            this.il_games.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // dialog_open
            // 
            this.dialog_open.DefaultExt = "exe";
            this.dialog_open.FileName = "*.exe";
            this.dialog_open.Filter = "All Files|*.*";
            this.dialog_open.RestoreDirectory = true;
            this.dialog_open.SupportMultiDottedExtensions = true;
            // 
            // tabc_shortcut
            // 
            this.tabc_shortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabc_shortcut.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabc_shortcut.Controls.Add(this.tabp_display);
            this.tabc_shortcut.Controls.Add(this.tabp_audio);
            this.tabc_shortcut.Controls.Add(this.tabp_before);
            this.tabc_shortcut.Controls.Add(this.tabp_game);
            this.tabc_shortcut.Controls.Add(this.tabp_after);
            this.tabc_shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabc_shortcut.HotTrack = true;
            this.tabc_shortcut.Location = new System.Drawing.Point(12, 62);
            this.tabc_shortcut.Name = "tabc_shortcut";
            this.tabc_shortcut.SelectedIndex = 0;
            this.tabc_shortcut.ShowToolTips = true;
            this.tabc_shortcut.Size = new System.Drawing.Size(1090, 630);
            this.tabc_shortcut.TabIndex = 28;
            // 
            // tabp_display
            // 
            this.tabp_display.BackColor = System.Drawing.Color.Black;
            this.tabp_display.Controls.Add(this.lbl_profile_shown_subtitle);
            this.tabp_display.Controls.Add(this.lbl_profile_shown);
            this.tabp_display.Controls.Add(this.ilv_saved_profiles);
            this.tabp_display.Controls.Add(this.dv_profile);
            this.tabp_display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_display.ForeColor = System.Drawing.Color.White;
            this.tabp_display.Location = new System.Drawing.Point(4, 32);
            this.tabp_display.Name = "tabp_display";
            this.tabp_display.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_display.Size = new System.Drawing.Size(1082, 594);
            this.tabp_display.TabIndex = 0;
            this.tabp_display.Text = "1. Choose Display Profile";
            this.tabp_display.ToolTipText = "Choose which previously saved Display Profile you will use with this shortcut.";
            // 
            // lbl_profile_shown_subtitle
            // 
            this.lbl_profile_shown_subtitle.AutoSize = true;
            this.lbl_profile_shown_subtitle.BackColor = System.Drawing.Color.DimGray;
            this.lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown_subtitle.Location = new System.Drawing.Point(18, 49);
            this.lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            this.lbl_profile_shown_subtitle.Size = new System.Drawing.Size(943, 20);
            this.lbl_profile_shown_subtitle.TabIndex = 26;
            this.lbl_profile_shown_subtitle.Text = "Please go back to the startup window, click on the \'Setup Display Profile\' button" +
    ", save a new Display Profile and then come back here.";
            // 
            // lbl_profile_shown
            // 
            this.lbl_profile_shown.AutoSize = true;
            this.lbl_profile_shown.BackColor = System.Drawing.Color.DimGray;
            this.lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown.Location = new System.Drawing.Point(18, 20);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(396, 29);
            this.lbl_profile_shown.TabIndex = 25;
            this.lbl_profile_shown.Text = "No Saved Display Profiles Available";
            // 
            // ilv_saved_profiles
            // 
            this.ilv_saved_profiles.AllowCheckBoxClick = false;
            this.ilv_saved_profiles.AllowColumnClick = false;
            this.ilv_saved_profiles.AllowColumnResize = false;
            this.ilv_saved_profiles.AllowItemReorder = false;
            this.ilv_saved_profiles.AllowPaneResize = false;
            this.ilv_saved_profiles.Colors = new Manina.Windows.Forms.ImageListViewColor(resources.GetString("ilv_saved_profiles.Colors"));
            this.ilv_saved_profiles.Location = new System.Drawing.Point(0, 466);
            this.ilv_saved_profiles.MultiSelect = false;
            this.ilv_saved_profiles.Name = "ilv_saved_profiles";
            this.ilv_saved_profiles.PersistentCacheDirectory = "";
            this.ilv_saved_profiles.PersistentCacheSize = ((long)(100));
            this.ilv_saved_profiles.Size = new System.Drawing.Size(1086, 128);
            this.ilv_saved_profiles.TabIndex = 24;
            this.ilv_saved_profiles.UseWIC = true;
            this.ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            this.ilv_saved_profiles.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_saved_profiles_ItemClick);
            // 
            // dv_profile
            // 
            this.dv_profile.BackColor = System.Drawing.Color.DimGray;
            this.dv_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(0, 0);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(1082, 467);
            this.dv_profile.TabIndex = 23;
            // 
            // tabp_audio
            // 
            this.tabp_audio.BackColor = System.Drawing.Color.Black;
            this.tabp_audio.Controls.Add(this.gb_audio_volume);
            this.tabp_audio.Controls.Add(this.btn_rescan_audio);
            this.tabp_audio.Controls.Add(this.cb_audio_device);
            this.tabp_audio.Controls.Add(this.rb_change_audio);
            this.tabp_audio.Controls.Add(this.rb_no_change_audio);
            this.tabp_audio.Location = new System.Drawing.Point(4, 32);
            this.tabp_audio.Name = "tabp_audio";
            this.tabp_audio.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_audio.Size = new System.Drawing.Size(1082, 594);
            this.tabp_audio.TabIndex = 4;
            this.tabp_audio.Text = "2. Choose Audio";
            // 
            // gb_audio_volume
            // 
            this.gb_audio_volume.Controls.Add(this.rb_set_audio_volume);
            this.gb_audio_volume.Controls.Add(this.rb_keep_audio_volume);
            this.gb_audio_volume.Controls.Add(this.lbl_audio_volume);
            this.gb_audio_volume.Controls.Add(this.nud_audio_volume);
            this.gb_audio_volume.ForeColor = System.Drawing.Color.White;
            this.gb_audio_volume.Location = new System.Drawing.Point(392, 240);
            this.gb_audio_volume.Name = "gb_audio_volume";
            this.gb_audio_volume.Size = new System.Drawing.Size(429, 147);
            this.gb_audio_volume.TabIndex = 10;
            this.gb_audio_volume.TabStop = false;
            this.gb_audio_volume.Text = "Audio Output Volume";
            this.gb_audio_volume.Visible = false;
            // 
            // rb_set_audio_volume
            // 
            this.rb_set_audio_volume.AutoSize = true;
            this.rb_set_audio_volume.ForeColor = System.Drawing.Color.White;
            this.rb_set_audio_volume.Location = new System.Drawing.Point(38, 84);
            this.rb_set_audio_volume.Name = "rb_set_audio_volume";
            this.rb_set_audio_volume.Size = new System.Drawing.Size(167, 24);
            this.rb_set_audio_volume.TabIndex = 13;
            this.rb_set_audio_volume.Text = "Set audio volume at";
            this.rb_set_audio_volume.UseVisualStyleBackColor = true;
            this.rb_set_audio_volume.CheckedChanged += new System.EventHandler(this.rb_set_audio_volume_CheckedChanged);
            // 
            // rb_keep_audio_volume
            // 
            this.rb_keep_audio_volume.AutoSize = true;
            this.rb_keep_audio_volume.Checked = true;
            this.rb_keep_audio_volume.ForeColor = System.Drawing.Color.White;
            this.rb_keep_audio_volume.Location = new System.Drawing.Point(38, 41);
            this.rb_keep_audio_volume.Name = "rb_keep_audio_volume";
            this.rb_keep_audio_volume.Size = new System.Drawing.Size(203, 24);
            this.rb_keep_audio_volume.TabIndex = 12;
            this.rb_keep_audio_volume.TabStop = true;
            this.rb_keep_audio_volume.Text = "Leave audio volume as is";
            this.rb_keep_audio_volume.UseVisualStyleBackColor = true;
            this.rb_keep_audio_volume.CheckedChanged += new System.EventHandler(this.rb_keep_audio_volume_CheckedChanged);
            // 
            // lbl_audio_volume
            // 
            this.lbl_audio_volume.AutoSize = true;
            this.lbl_audio_volume.ForeColor = System.Drawing.Color.White;
            this.lbl_audio_volume.Location = new System.Drawing.Point(275, 86);
            this.lbl_audio_volume.Name = "lbl_audio_volume";
            this.lbl_audio_volume.Size = new System.Drawing.Size(63, 20);
            this.lbl_audio_volume.TabIndex = 11;
            this.lbl_audio_volume.Text = "percent";
            // 
            // nud_audio_volume
            // 
            this.nud_audio_volume.Enabled = false;
            this.nud_audio_volume.Location = new System.Drawing.Point(209, 84);
            this.nud_audio_volume.Name = "nud_audio_volume";
            this.nud_audio_volume.Size = new System.Drawing.Size(60, 26);
            this.nud_audio_volume.TabIndex = 10;
            this.nud_audio_volume.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // btn_rescan_audio
            // 
            this.btn_rescan_audio.Enabled = false;
            this.btn_rescan_audio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_rescan_audio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_rescan_audio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rescan_audio.ForeColor = System.Drawing.Color.White;
            this.btn_rescan_audio.Location = new System.Drawing.Point(827, 186);
            this.btn_rescan_audio.Name = "btn_rescan_audio";
            this.btn_rescan_audio.Size = new System.Drawing.Size(71, 28);
            this.btn_rescan_audio.TabIndex = 3;
            this.btn_rescan_audio.Text = "rescan";
            this.btn_rescan_audio.UseVisualStyleBackColor = true;
            this.btn_rescan_audio.Click += new System.EventHandler(this.btn_rescan_audio_Click);
            // 
            // cb_audio_device
            // 
            this.cb_audio_device.Enabled = false;
            this.cb_audio_device.FormattingEnabled = true;
            this.cb_audio_device.Location = new System.Drawing.Point(392, 186);
            this.cb_audio_device.Name = "cb_audio_device";
            this.cb_audio_device.Size = new System.Drawing.Size(429, 28);
            this.cb_audio_device.TabIndex = 2;
            this.cb_audio_device.SelectedIndexChanged += new System.EventHandler(this.cb_audio_device_SelectedIndexChanged);
            // 
            // rb_change_audio
            // 
            this.rb_change_audio.AutoSize = true;
            this.rb_change_audio.ForeColor = System.Drawing.Color.White;
            this.rb_change_audio.Location = new System.Drawing.Point(188, 186);
            this.rb_change_audio.Name = "rb_change_audio";
            this.rb_change_audio.Size = new System.Drawing.Size(198, 24);
            this.rb_change_audio.TabIndex = 1;
            this.rb_change_audio.Text = "Change audio output to:";
            this.rb_change_audio.UseVisualStyleBackColor = true;
            this.rb_change_audio.CheckedChanged += new System.EventHandler(this.rb_change_audio_CheckedChanged);
            // 
            // rb_no_change_audio
            // 
            this.rb_no_change_audio.AutoSize = true;
            this.rb_no_change_audio.Checked = true;
            this.rb_no_change_audio.ForeColor = System.Drawing.Color.White;
            this.rb_no_change_audio.Location = new System.Drawing.Point(188, 114);
            this.rb_no_change_audio.Name = "rb_no_change_audio";
            this.rb_no_change_audio.Size = new System.Drawing.Size(215, 24);
            this.rb_no_change_audio.TabIndex = 0;
            this.rb_no_change_audio.TabStop = true;
            this.rb_no_change_audio.Text = "Don\'t change audio output";
            this.rb_no_change_audio.UseVisualStyleBackColor = true;
            this.rb_no_change_audio.CheckedChanged += new System.EventHandler(this.rb_no_change_audio_CheckedChanged);
            // 
            // tabp_before
            // 
            this.tabp_before.BackColor = System.Drawing.Color.Black;
            this.tabp_before.Controls.Add(this.pnl_start_program4);
            this.tabp_before.Controls.Add(this.pnl_start_program3);
            this.tabp_before.Controls.Add(this.pnl_start_program2);
            this.tabp_before.Controls.Add(this.pnl_start_program1);
            this.tabp_before.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_before.ForeColor = System.Drawing.Color.White;
            this.tabp_before.Location = new System.Drawing.Point(4, 32);
            this.tabp_before.Name = "tabp_before";
            this.tabp_before.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_before.Size = new System.Drawing.Size(1082, 594);
            this.tabp_before.TabIndex = 1;
            this.tabp_before.Text = "3. Choose what happens before";
            // 
            // pnl_start_program4
            // 
            this.pnl_start_program4.Controls.Add(this.cb_start_program4);
            this.pnl_start_program4.Controls.Add(this.txt_start_program4);
            this.pnl_start_program4.Controls.Add(this.cb_start_program_close4);
            this.pnl_start_program4.Controls.Add(this.btn_start_program4);
            this.pnl_start_program4.Controls.Add(this.txt_start_program_args4);
            this.pnl_start_program4.Controls.Add(this.cb_start_program_pass_args4);
            this.pnl_start_program4.Controls.Add(this.lbl_start_program4);
            this.pnl_start_program4.Location = new System.Drawing.Point(48, 443);
            this.pnl_start_program4.Name = "pnl_start_program4";
            this.pnl_start_program4.Size = new System.Drawing.Size(959, 124);
            this.pnl_start_program4.TabIndex = 19;
            // 
            // cb_start_program4
            // 
            this.cb_start_program4.Location = new System.Drawing.Point(21, 18);
            this.cb_start_program4.Name = "cb_start_program4";
            this.cb_start_program4.Size = new System.Drawing.Size(15, 14);
            this.cb_start_program4.TabIndex = 18;
            this.cb_start_program4.UseVisualStyleBackColor = true;
            this.cb_start_program4.CheckedChanged += new System.EventHandler(this.cb_start_program4_CheckedChanged);
            // 
            // txt_start_program4
            // 
            this.txt_start_program4.Location = new System.Drawing.Point(300, 11);
            this.txt_start_program4.Name = "txt_start_program4";
            this.txt_start_program4.Size = new System.Drawing.Size(535, 26);
            this.txt_start_program4.TabIndex = 17;
            this.txt_start_program4.Visible = false;
            // 
            // cb_start_program_close4
            // 
            this.cb_start_program_close4.AutoSize = true;
            this.cb_start_program_close4.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close4.Location = new System.Drawing.Point(167, 87);
            this.cb_start_program_close4.Name = "cb_start_program_close4";
            this.cb_start_program_close4.Size = new System.Drawing.Size(344, 24);
            this.cb_start_program_close4.TabIndex = 16;
            this.cb_start_program_close4.Text = "Close program when you finish playing Game";
            this.cb_start_program_close4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close4.UseVisualStyleBackColor = true;
            this.cb_start_program_close4.Visible = false;
            // 
            // btn_start_program4
            // 
            this.btn_start_program4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program4.ForeColor = System.Drawing.Color.White;
            this.btn_start_program4.Location = new System.Drawing.Point(851, 10);
            this.btn_start_program4.Name = "btn_start_program4";
            this.btn_start_program4.Size = new System.Drawing.Size(85, 27);
            this.btn_start_program4.TabIndex = 15;
            this.btn_start_program4.Text = "Choose";
            this.btn_start_program4.UseVisualStyleBackColor = true;
            this.btn_start_program4.Visible = false;
            this.btn_start_program4.Click += new System.EventHandler(this.btn_start_program4_Click);
            // 
            // txt_start_program_args4
            // 
            this.txt_start_program_args4.Location = new System.Drawing.Point(397, 50);
            this.txt_start_program_args4.Name = "txt_start_program_args4";
            this.txt_start_program_args4.Size = new System.Drawing.Size(540, 26);
            this.txt_start_program_args4.TabIndex = 14;
            this.txt_start_program_args4.Visible = false;
            // 
            // cb_start_program_pass_args4
            // 
            this.cb_start_program_pass_args4.AutoSize = true;
            this.cb_start_program_pass_args4.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args4.Location = new System.Drawing.Point(167, 52);
            this.cb_start_program_pass_args4.Name = "cb_start_program_pass_args4";
            this.cb_start_program_pass_args4.Size = new System.Drawing.Size(228, 24);
            this.cb_start_program_pass_args4.TabIndex = 13;
            this.cb_start_program_pass_args4.Text = "Pass arguments to program:";
            this.cb_start_program_pass_args4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_pass_args4.UseVisualStyleBackColor = true;
            this.cb_start_program_pass_args4.Visible = false;
            this.cb_start_program_pass_args4.CheckedChanged += new System.EventHandler(this.cb_start_program_pass_args4_CheckedChanged);
            // 
            // lbl_start_program4
            // 
            this.lbl_start_program4.AutoSize = true;
            this.lbl_start_program4.Location = new System.Drawing.Point(46, 14);
            this.lbl_start_program4.Name = "lbl_start_program4";
            this.lbl_start_program4.Size = new System.Drawing.Size(244, 20);
            this.lbl_start_program4.TabIndex = 0;
            this.lbl_start_program4.Text = "Choose a program to start fourth:";
            // 
            // pnl_start_program3
            // 
            this.pnl_start_program3.Controls.Add(this.cb_start_program3);
            this.pnl_start_program3.Controls.Add(this.txt_start_program3);
            this.pnl_start_program3.Controls.Add(this.cb_start_program_close3);
            this.pnl_start_program3.Controls.Add(this.btn_start_program3);
            this.pnl_start_program3.Controls.Add(this.txt_start_program_args3);
            this.pnl_start_program3.Controls.Add(this.cb_start_program_pass_args3);
            this.pnl_start_program3.Controls.Add(this.lbl_start_program3);
            this.pnl_start_program3.Location = new System.Drawing.Point(48, 306);
            this.pnl_start_program3.Name = "pnl_start_program3";
            this.pnl_start_program3.Size = new System.Drawing.Size(959, 124);
            this.pnl_start_program3.TabIndex = 18;
            // 
            // cb_start_program3
            // 
            this.cb_start_program3.Location = new System.Drawing.Point(21, 18);
            this.cb_start_program3.Name = "cb_start_program3";
            this.cb_start_program3.Size = new System.Drawing.Size(15, 14);
            this.cb_start_program3.TabIndex = 18;
            this.cb_start_program3.UseVisualStyleBackColor = true;
            this.cb_start_program3.CheckedChanged += new System.EventHandler(this.cb_start_program3_CheckedChanged);
            // 
            // txt_start_program3
            // 
            this.txt_start_program3.Location = new System.Drawing.Point(300, 11);
            this.txt_start_program3.Name = "txt_start_program3";
            this.txt_start_program3.Size = new System.Drawing.Size(535, 26);
            this.txt_start_program3.TabIndex = 17;
            this.txt_start_program3.Visible = false;
            // 
            // cb_start_program_close3
            // 
            this.cb_start_program_close3.AutoSize = true;
            this.cb_start_program_close3.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close3.Location = new System.Drawing.Point(167, 87);
            this.cb_start_program_close3.Name = "cb_start_program_close3";
            this.cb_start_program_close3.Size = new System.Drawing.Size(344, 24);
            this.cb_start_program_close3.TabIndex = 16;
            this.cb_start_program_close3.Text = "Close program when you finish playing Game";
            this.cb_start_program_close3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close3.UseVisualStyleBackColor = true;
            this.cb_start_program_close3.Visible = false;
            // 
            // btn_start_program3
            // 
            this.btn_start_program3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program3.ForeColor = System.Drawing.Color.White;
            this.btn_start_program3.Location = new System.Drawing.Point(851, 10);
            this.btn_start_program3.Name = "btn_start_program3";
            this.btn_start_program3.Size = new System.Drawing.Size(85, 27);
            this.btn_start_program3.TabIndex = 15;
            this.btn_start_program3.Text = "Choose";
            this.btn_start_program3.UseVisualStyleBackColor = true;
            this.btn_start_program3.Visible = false;
            this.btn_start_program3.Click += new System.EventHandler(this.btn_start_program3_Click);
            // 
            // txt_start_program_args3
            // 
            this.txt_start_program_args3.Location = new System.Drawing.Point(397, 50);
            this.txt_start_program_args3.Name = "txt_start_program_args3";
            this.txt_start_program_args3.Size = new System.Drawing.Size(540, 26);
            this.txt_start_program_args3.TabIndex = 14;
            this.txt_start_program_args3.Visible = false;
            // 
            // cb_start_program_pass_args3
            // 
            this.cb_start_program_pass_args3.AutoSize = true;
            this.cb_start_program_pass_args3.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args3.Location = new System.Drawing.Point(167, 52);
            this.cb_start_program_pass_args3.Name = "cb_start_program_pass_args3";
            this.cb_start_program_pass_args3.Size = new System.Drawing.Size(228, 24);
            this.cb_start_program_pass_args3.TabIndex = 13;
            this.cb_start_program_pass_args3.Text = "Pass arguments to program:";
            this.cb_start_program_pass_args3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_pass_args3.UseVisualStyleBackColor = true;
            this.cb_start_program_pass_args3.Visible = false;
            this.cb_start_program_pass_args3.CheckedChanged += new System.EventHandler(this.cb_start_program_pass_args3_CheckedChanged);
            // 
            // lbl_start_program3
            // 
            this.lbl_start_program3.AutoSize = true;
            this.lbl_start_program3.Location = new System.Drawing.Point(46, 14);
            this.lbl_start_program3.Name = "lbl_start_program3";
            this.lbl_start_program3.Size = new System.Drawing.Size(233, 20);
            this.lbl_start_program3.TabIndex = 0;
            this.lbl_start_program3.Text = "Choose a program to start third:";
            // 
            // pnl_start_program2
            // 
            this.pnl_start_program2.Controls.Add(this.cb_start_program2);
            this.pnl_start_program2.Controls.Add(this.txt_start_program2);
            this.pnl_start_program2.Controls.Add(this.cb_start_program_close2);
            this.pnl_start_program2.Controls.Add(this.btn_start_program2);
            this.pnl_start_program2.Controls.Add(this.txt_start_program_args2);
            this.pnl_start_program2.Controls.Add(this.cb_start_program_pass_args2);
            this.pnl_start_program2.Controls.Add(this.lbl_start_program2);
            this.pnl_start_program2.Location = new System.Drawing.Point(48, 167);
            this.pnl_start_program2.Name = "pnl_start_program2";
            this.pnl_start_program2.Size = new System.Drawing.Size(959, 124);
            this.pnl_start_program2.TabIndex = 18;
            // 
            // cb_start_program2
            // 
            this.cb_start_program2.Location = new System.Drawing.Point(21, 18);
            this.cb_start_program2.Name = "cb_start_program2";
            this.cb_start_program2.Size = new System.Drawing.Size(15, 14);
            this.cb_start_program2.TabIndex = 18;
            this.cb_start_program2.UseVisualStyleBackColor = true;
            this.cb_start_program2.CheckedChanged += new System.EventHandler(this.cb_start_program2_CheckedChanged);
            // 
            // txt_start_program2
            // 
            this.txt_start_program2.Location = new System.Drawing.Point(301, 11);
            this.txt_start_program2.Name = "txt_start_program2";
            this.txt_start_program2.Size = new System.Drawing.Size(534, 26);
            this.txt_start_program2.TabIndex = 17;
            this.txt_start_program2.Visible = false;
            // 
            // cb_start_program_close2
            // 
            this.cb_start_program_close2.AutoSize = true;
            this.cb_start_program_close2.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close2.Location = new System.Drawing.Point(167, 87);
            this.cb_start_program_close2.Name = "cb_start_program_close2";
            this.cb_start_program_close2.Size = new System.Drawing.Size(344, 24);
            this.cb_start_program_close2.TabIndex = 16;
            this.cb_start_program_close2.Text = "Close program when you finish playing Game";
            this.cb_start_program_close2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close2.UseVisualStyleBackColor = true;
            this.cb_start_program_close2.Visible = false;
            // 
            // btn_start_program2
            // 
            this.btn_start_program2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program2.ForeColor = System.Drawing.Color.White;
            this.btn_start_program2.Location = new System.Drawing.Point(852, 10);
            this.btn_start_program2.Name = "btn_start_program2";
            this.btn_start_program2.Size = new System.Drawing.Size(85, 27);
            this.btn_start_program2.TabIndex = 15;
            this.btn_start_program2.Text = "Choose";
            this.btn_start_program2.UseVisualStyleBackColor = true;
            this.btn_start_program2.Visible = false;
            this.btn_start_program2.Click += new System.EventHandler(this.btn_start_program2_Click);
            // 
            // txt_start_program_args2
            // 
            this.txt_start_program_args2.Location = new System.Drawing.Point(397, 50);
            this.txt_start_program_args2.Name = "txt_start_program_args2";
            this.txt_start_program_args2.Size = new System.Drawing.Size(540, 26);
            this.txt_start_program_args2.TabIndex = 14;
            this.txt_start_program_args2.Visible = false;
            // 
            // cb_start_program_pass_args2
            // 
            this.cb_start_program_pass_args2.AutoSize = true;
            this.cb_start_program_pass_args2.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args2.Location = new System.Drawing.Point(167, 52);
            this.cb_start_program_pass_args2.Name = "cb_start_program_pass_args2";
            this.cb_start_program_pass_args2.Size = new System.Drawing.Size(228, 24);
            this.cb_start_program_pass_args2.TabIndex = 13;
            this.cb_start_program_pass_args2.Text = "Pass arguments to program:";
            this.cb_start_program_pass_args2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_pass_args2.UseVisualStyleBackColor = true;
            this.cb_start_program_pass_args2.Visible = false;
            this.cb_start_program_pass_args2.CheckedChanged += new System.EventHandler(this.cb_start_program_pass_args2_CheckedChanged);
            // 
            // lbl_start_program2
            // 
            this.lbl_start_program2.AutoSize = true;
            this.lbl_start_program2.Location = new System.Drawing.Point(46, 14);
            this.lbl_start_program2.Name = "lbl_start_program2";
            this.lbl_start_program2.Size = new System.Drawing.Size(254, 20);
            this.lbl_start_program2.TabIndex = 0;
            this.lbl_start_program2.Text = "Choose a program to start second:";
            // 
            // pnl_start_program1
            // 
            this.pnl_start_program1.Controls.Add(this.cb_start_program1);
            this.pnl_start_program1.Controls.Add(this.txt_start_program1);
            this.pnl_start_program1.Controls.Add(this.cb_start_program_close1);
            this.pnl_start_program1.Controls.Add(this.btn_start_program1);
            this.pnl_start_program1.Controls.Add(this.txt_start_program_args1);
            this.pnl_start_program1.Controls.Add(this.cb_start_program_pass_args1);
            this.pnl_start_program1.Controls.Add(this.lbl_start_program1);
            this.pnl_start_program1.Location = new System.Drawing.Point(48, 28);
            this.pnl_start_program1.Name = "pnl_start_program1";
            this.pnl_start_program1.Size = new System.Drawing.Size(959, 124);
            this.pnl_start_program1.TabIndex = 0;
            // 
            // cb_start_program1
            // 
            this.cb_start_program1.Location = new System.Drawing.Point(21, 18);
            this.cb_start_program1.Name = "cb_start_program1";
            this.cb_start_program1.Size = new System.Drawing.Size(15, 14);
            this.cb_start_program1.TabIndex = 0;
            this.cb_start_program1.UseVisualStyleBackColor = true;
            this.cb_start_program1.CheckedChanged += new System.EventHandler(this.cb_start_program1_CheckedChanged);
            // 
            // txt_start_program1
            // 
            this.txt_start_program1.Location = new System.Drawing.Point(300, 11);
            this.txt_start_program1.Name = "txt_start_program1";
            this.txt_start_program1.Size = new System.Drawing.Size(535, 26);
            this.txt_start_program1.TabIndex = 17;
            this.txt_start_program1.Visible = false;
            // 
            // cb_start_program_close1
            // 
            this.cb_start_program_close1.AutoSize = true;
            this.cb_start_program_close1.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close1.Location = new System.Drawing.Point(167, 87);
            this.cb_start_program_close1.Name = "cb_start_program_close1";
            this.cb_start_program_close1.Size = new System.Drawing.Size(344, 24);
            this.cb_start_program_close1.TabIndex = 16;
            this.cb_start_program_close1.Text = "Close program when you finish playing Game";
            this.cb_start_program_close1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close1.UseVisualStyleBackColor = true;
            this.cb_start_program_close1.Visible = false;
            // 
            // btn_start_program1
            // 
            this.btn_start_program1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program1.ForeColor = System.Drawing.Color.White;
            this.btn_start_program1.Location = new System.Drawing.Point(851, 10);
            this.btn_start_program1.Name = "btn_start_program1";
            this.btn_start_program1.Size = new System.Drawing.Size(85, 27);
            this.btn_start_program1.TabIndex = 15;
            this.btn_start_program1.Text = "Choose";
            this.btn_start_program1.UseVisualStyleBackColor = true;
            this.btn_start_program1.Visible = false;
            this.btn_start_program1.Click += new System.EventHandler(this.btn_start_program1_Click);
            // 
            // txt_start_program_args1
            // 
            this.txt_start_program_args1.Location = new System.Drawing.Point(397, 50);
            this.txt_start_program_args1.Name = "txt_start_program_args1";
            this.txt_start_program_args1.Size = new System.Drawing.Size(540, 26);
            this.txt_start_program_args1.TabIndex = 14;
            this.txt_start_program_args1.Visible = false;
            // 
            // cb_start_program_pass_args1
            // 
            this.cb_start_program_pass_args1.AutoSize = true;
            this.cb_start_program_pass_args1.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args1.Location = new System.Drawing.Point(167, 52);
            this.cb_start_program_pass_args1.Name = "cb_start_program_pass_args1";
            this.cb_start_program_pass_args1.Size = new System.Drawing.Size(228, 24);
            this.cb_start_program_pass_args1.TabIndex = 13;
            this.cb_start_program_pass_args1.Text = "Pass arguments to program:";
            this.cb_start_program_pass_args1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_pass_args1.UseVisualStyleBackColor = true;
            this.cb_start_program_pass_args1.Visible = false;
            this.cb_start_program_pass_args1.CheckedChanged += new System.EventHandler(this.cb_start_program_pass_args1_CheckedChanged);
            // 
            // lbl_start_program1
            // 
            this.lbl_start_program1.AutoSize = true;
            this.lbl_start_program1.Location = new System.Drawing.Point(46, 14);
            this.lbl_start_program1.Name = "lbl_start_program1";
            this.lbl_start_program1.Size = new System.Drawing.Size(228, 20);
            this.lbl_start_program1.TabIndex = 0;
            this.lbl_start_program1.Text = "Choose a program to start first:";
            // 
            // tabp_game
            // 
            this.tabp_game.BackColor = System.Drawing.Color.Black;
            this.tabp_game.Controls.Add(this.p_standalone);
            this.tabp_game.Controls.Add(this.rb_standalone);
            this.tabp_game.Controls.Add(this.rb_no_game);
            this.tabp_game.Controls.Add(this.p_game);
            this.tabp_game.Controls.Add(this.rb_launcher);
            this.tabp_game.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_game.ForeColor = System.Drawing.Color.White;
            this.tabp_game.Location = new System.Drawing.Point(4, 32);
            this.tabp_game.Name = "tabp_game";
            this.tabp_game.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_game.Size = new System.Drawing.Size(1082, 594);
            this.tabp_game.TabIndex = 2;
            this.tabp_game.Text = "4. Choose Game to start";
            // 
            // p_standalone
            // 
            this.p_standalone.Controls.Add(this.btn_exe_to_start);
            this.p_standalone.Controls.Add(this.txt_args_executable);
            this.p_standalone.Controls.Add(this.cb_args_executable);
            this.p_standalone.Controls.Add(this.btn_app_different_executable);
            this.p_standalone.Controls.Add(this.txt_alternative_executable);
            this.p_standalone.Controls.Add(this.rb_wait_alternative_executable);
            this.p_standalone.Controls.Add(this.rb_wait_executable);
            this.p_standalone.Controls.Add(this.txt_executable);
            this.p_standalone.Controls.Add(this.lbl_app_executable);
            this.p_standalone.Controls.Add(this.label2);
            this.p_standalone.Controls.Add(this.nud_timeout_executable);
            this.p_standalone.Enabled = false;
            this.p_standalone.Location = new System.Drawing.Point(35, 415);
            this.p_standalone.Name = "p_standalone";
            this.p_standalone.Size = new System.Drawing.Size(1006, 160);
            this.p_standalone.TabIndex = 10;
            // 
            // btn_exe_to_start
            // 
            this.btn_exe_to_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_exe_to_start.ForeColor = System.Drawing.Color.White;
            this.btn_exe_to_start.Location = new System.Drawing.Point(593, 10);
            this.btn_exe_to_start.Name = "btn_exe_to_start";
            this.btn_exe_to_start.Size = new System.Drawing.Size(85, 27);
            this.btn_exe_to_start.TabIndex = 12;
            this.btn_exe_to_start.Text = "Choose";
            this.btn_exe_to_start.UseVisualStyleBackColor = true;
            this.btn_exe_to_start.Click += new System.EventHandler(this.btn_exe_to_start_Click);
            // 
            // txt_args_executable
            // 
            this.txt_args_executable.Enabled = false;
            this.txt_args_executable.Location = new System.Drawing.Point(425, 46);
            this.txt_args_executable.Name = "txt_args_executable";
            this.txt_args_executable.Size = new System.Drawing.Size(540, 26);
            this.txt_args_executable.TabIndex = 11;
            // 
            // cb_args_executable
            // 
            this.cb_args_executable.AutoSize = true;
            this.cb_args_executable.ForeColor = System.Drawing.Color.White;
            this.cb_args_executable.Location = new System.Drawing.Point(171, 48);
            this.cb_args_executable.Name = "cb_args_executable";
            this.cb_args_executable.Size = new System.Drawing.Size(248, 24);
            this.cb_args_executable.TabIndex = 10;
            this.cb_args_executable.Text = "Pass arguments to Executable:";
            this.cb_args_executable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_args_executable.UseVisualStyleBackColor = true;
            this.cb_args_executable.CheckedChanged += new System.EventHandler(this.cb_args_executable_CheckedChanged);
            this.cb_args_executable.Paint += new System.Windows.Forms.PaintEventHandler(this.checkbox_Paint);
            // 
            // btn_app_different_executable
            // 
            this.btn_app_different_executable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_app_different_executable.ForeColor = System.Drawing.Color.White;
            this.btn_app_different_executable.Location = new System.Drawing.Point(877, 115);
            this.btn_app_different_executable.Name = "btn_app_different_executable";
            this.btn_app_different_executable.Size = new System.Drawing.Size(85, 27);
            this.btn_app_different_executable.TabIndex = 9;
            this.btn_app_different_executable.Text = "Choose";
            this.btn_app_different_executable.UseVisualStyleBackColor = true;
            this.btn_app_different_executable.Click += new System.EventHandler(this.btn_app_different_executable_Click);
            // 
            // txt_alternative_executable
            // 
            this.txt_alternative_executable.Enabled = false;
            this.txt_alternative_executable.Location = new System.Drawing.Point(493, 116);
            this.txt_alternative_executable.Name = "txt_alternative_executable";
            this.txt_alternative_executable.Size = new System.Drawing.Size(378, 26);
            this.txt_alternative_executable.TabIndex = 4;
            this.txt_alternative_executable.TextChanged += new System.EventHandler(this.txt_different_executable_TextChanged);
            // 
            // rb_wait_alternative_executable
            // 
            this.rb_wait_alternative_executable.AutoSize = true;
            this.rb_wait_alternative_executable.ForeColor = System.Drawing.Color.White;
            this.rb_wait_alternative_executable.Location = new System.Drawing.Point(23, 118);
            this.rb_wait_alternative_executable.Name = "rb_wait_alternative_executable";
            this.rb_wait_alternative_executable.Size = new System.Drawing.Size(468, 24);
            this.rb_wait_alternative_executable.TabIndex = 8;
            this.rb_wait_alternative_executable.Text = "Wait until an alternative executable is closed before continuing:";
            this.rb_wait_alternative_executable.UseVisualStyleBackColor = true;
            this.rb_wait_alternative_executable.CheckedChanged += new System.EventHandler(this.rb_wait_process_CheckedChanged);
            this.rb_wait_alternative_executable.Paint += new System.Windows.Forms.PaintEventHandler(this.radiobutton_Paint);
            // 
            // rb_wait_executable
            // 
            this.rb_wait_executable.AutoSize = true;
            this.rb_wait_executable.Checked = true;
            this.rb_wait_executable.ForeColor = System.Drawing.Color.White;
            this.rb_wait_executable.Location = new System.Drawing.Point(23, 87);
            this.rb_wait_executable.Name = "rb_wait_executable";
            this.rb_wait_executable.Size = new System.Drawing.Size(439, 24);
            this.rb_wait_executable.TabIndex = 7;
            this.rb_wait_executable.TabStop = true;
            this.rb_wait_executable.Text = "Wait until the executable above is closed before continuing";
            this.rb_wait_executable.UseVisualStyleBackColor = true;
            this.rb_wait_executable.CheckedChanged += new System.EventHandler(this.rb_wait_executable_CheckedChanged);
            this.rb_wait_executable.Paint += new System.Windows.Forms.PaintEventHandler(this.radiobutton_Paint);
            // 
            // txt_executable
            // 
            this.txt_executable.Location = new System.Drawing.Point(171, 10);
            this.txt_executable.Name = "txt_executable";
            this.txt_executable.Size = new System.Drawing.Size(416, 26);
            this.txt_executable.TabIndex = 1;
            this.txt_executable.TextChanged += new System.EventHandler(this.txt_executable_TextChanged);
            // 
            // lbl_app_executable
            // 
            this.lbl_app_executable.AutoSize = true;
            this.lbl_app_executable.ForeColor = System.Drawing.Color.White;
            this.lbl_app_executable.Location = new System.Drawing.Point(19, 13);
            this.lbl_app_executable.Name = "lbl_app_executable";
            this.lbl_app_executable.Size = new System.Drawing.Size(146, 20);
            this.lbl_app_executable.TabIndex = 0;
            this.lbl_app_executable.Text = "Executable to start:";
            this.lbl_app_executable.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbl_app_executable.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(819, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Timeout:";
            this.label2.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // nud_timeout_executable
            // 
            this.nud_timeout_executable.Location = new System.Drawing.Point(895, 10);
            this.nud_timeout_executable.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_timeout_executable.Name = "nud_timeout_executable";
            this.nud_timeout_executable.Size = new System.Drawing.Size(70, 26);
            this.nud_timeout_executable.TabIndex = 6;
            this.nud_timeout_executable.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // rb_standalone
            // 
            this.rb_standalone.AutoSize = true;
            this.rb_standalone.ForeColor = System.Drawing.Color.White;
            this.rb_standalone.Location = new System.Drawing.Point(16, 389);
            this.rb_standalone.Name = "rb_standalone";
            this.rb_standalone.Size = new System.Drawing.Size(222, 24);
            this.rb_standalone.TabIndex = 9;
            this.rb_standalone.Text = "Launch a Game executable";
            this.rb_standalone.UseVisualStyleBackColor = true;
            this.rb_standalone.CheckedChanged += new System.EventHandler(this.rb_standalone_CheckedChanged);
            // 
            // rb_no_game
            // 
            this.rb_no_game.AutoSize = true;
            this.rb_no_game.ForeColor = System.Drawing.Color.White;
            this.rb_no_game.Location = new System.Drawing.Point(15, 18);
            this.rb_no_game.Name = "rb_no_game";
            this.rb_no_game.Size = new System.Drawing.Size(162, 24);
            this.rb_no_game.TabIndex = 8;
            this.rb_no_game.Text = "Don\'t start a Game";
            this.rb_no_game.UseVisualStyleBackColor = true;
            this.rb_no_game.CheckedChanged += new System.EventHandler(this.rb_no_game_CheckedChanged);
            // 
            // p_game
            // 
            this.p_game.Controls.Add(this.txt_game_launcher);
            this.p_game.Controls.Add(this.txt_game_name);
            this.p_game.Controls.Add(this.lbl_game_library);
            this.p_game.Controls.Add(this.lbl_game_name);
            this.p_game.Controls.Add(this.btn_choose_game);
            this.p_game.Controls.Add(this.label1);
            this.p_game.Controls.Add(this.txt_args_game);
            this.p_game.Controls.Add(this.cb_args_game);
            this.p_game.Controls.Add(this.lbl_game_timeout);
            this.p_game.Controls.Add(this.nud_timeout_game);
            this.p_game.Controls.Add(this.lv_games);
            this.p_game.Location = new System.Drawing.Point(34, 98);
            this.p_game.Name = "p_game";
            this.p_game.Size = new System.Drawing.Size(1006, 253);
            this.p_game.TabIndex = 7;
            // 
            // txt_game_launcher
            // 
            this.txt_game_launcher.Location = new System.Drawing.Point(605, 76);
            this.txt_game_launcher.Name = "txt_game_launcher";
            this.txt_game_launcher.ReadOnly = true;
            this.txt_game_launcher.Size = new System.Drawing.Size(175, 26);
            this.txt_game_launcher.TabIndex = 23;
            // 
            // txt_game_name
            // 
            this.txt_game_name.Location = new System.Drawing.Point(605, 117);
            this.txt_game_name.Name = "txt_game_name";
            this.txt_game_name.ReadOnly = true;
            this.txt_game_name.Size = new System.Drawing.Size(360, 26);
            this.txt_game_name.TabIndex = 21;
            // 
            // lbl_game_library
            // 
            this.lbl_game_library.AutoSize = true;
            this.lbl_game_library.ForeColor = System.Drawing.Color.White;
            this.lbl_game_library.Location = new System.Drawing.Point(490, 79);
            this.lbl_game_library.Name = "lbl_game_library";
            this.lbl_game_library.Size = new System.Drawing.Size(108, 20);
            this.lbl_game_library.TabIndex = 18;
            this.lbl_game_library.Text = "Game Library:";
            this.lbl_game_library.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // lbl_game_name
            // 
            this.lbl_game_name.AutoSize = true;
            this.lbl_game_name.ForeColor = System.Drawing.Color.White;
            this.lbl_game_name.Location = new System.Drawing.Point(496, 120);
            this.lbl_game_name.Name = "lbl_game_name";
            this.lbl_game_name.Size = new System.Drawing.Size(103, 20);
            this.lbl_game_name.TabIndex = 17;
            this.lbl_game_name.Text = "Game Name:";
            this.lbl_game_name.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_game_name.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // btn_choose_game
            // 
            this.btn_choose_game.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_choose_game.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_choose_game.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_choose_game.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_choose_game.ForeColor = System.Drawing.Color.White;
            this.btn_choose_game.Location = new System.Drawing.Point(408, 117);
            this.btn_choose_game.Name = "btn_choose_game";
            this.btn_choose_game.Size = new System.Drawing.Size(40, 46);
            this.btn_choose_game.TabIndex = 16;
            this.btn_choose_game.Text = ">>";
            this.btn_choose_game.UseVisualStyleBackColor = true;
            this.btn_choose_game.Click += new System.EventHandler(this.btn_choose_game_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(19, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 20);
            this.label1.TabIndex = 15;
            this.label1.Text = "Games detected:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // txt_args_game
            // 
            this.txt_args_game.Enabled = false;
            this.txt_args_game.Location = new System.Drawing.Point(605, 179);
            this.txt_args_game.Name = "txt_args_game";
            this.txt_args_game.Size = new System.Drawing.Size(360, 26);
            this.txt_args_game.TabIndex = 13;
            // 
            // cb_args_game
            // 
            this.cb_args_game.AutoSize = true;
            this.cb_args_game.ForeColor = System.Drawing.Color.White;
            this.cb_args_game.Location = new System.Drawing.Point(605, 149);
            this.cb_args_game.Name = "cb_args_game";
            this.cb_args_game.Size = new System.Drawing.Size(213, 24);
            this.cb_args_game.TabIndex = 12;
            this.cb_args_game.Text = "Pass arguments to Game:";
            this.cb_args_game.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_args_game.UseVisualStyleBackColor = true;
            this.cb_args_game.CheckedChanged += new System.EventHandler(this.cb_args_game_CheckedChanged);
            this.cb_args_game.Paint += new System.Windows.Forms.PaintEventHandler(this.checkbox_Paint);
            // 
            // lbl_game_timeout
            // 
            this.lbl_game_timeout.AutoSize = true;
            this.lbl_game_timeout.ForeColor = System.Drawing.Color.White;
            this.lbl_game_timeout.Location = new System.Drawing.Point(819, 79);
            this.lbl_game_timeout.Name = "lbl_game_timeout";
            this.lbl_game_timeout.Size = new System.Drawing.Size(70, 20);
            this.lbl_game_timeout.TabIndex = 4;
            this.lbl_game_timeout.Text = "Timeout:";
            this.lbl_game_timeout.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // nud_timeout_game
            // 
            this.nud_timeout_game.Location = new System.Drawing.Point(895, 77);
            this.nud_timeout_game.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_timeout_game.Name = "nud_timeout_game";
            this.nud_timeout_game.Size = new System.Drawing.Size(70, 26);
            this.nud_timeout_game.TabIndex = 5;
            this.nud_timeout_game.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // lv_games
            // 
            this.lv_games.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clm_images,
            this.clm_name});
            this.lv_games.HideSelection = false;
            this.lv_games.LargeImageList = this.il_games;
            this.lv_games.Location = new System.Drawing.Point(23, 30);
            this.lv_games.Name = "lv_games";
            this.lv_games.Size = new System.Drawing.Size(338, 217);
            this.lv_games.SmallImageList = this.il_games;
            this.lv_games.TabIndex = 22;
            this.lv_games.UseCompatibleStateImageBehavior = false;
            this.lv_games.DoubleClick += new System.EventHandler(this.btn_choose_game_Click);
            // 
            // rb_launcher
            // 
            this.rb_launcher.AutoSize = true;
            this.rb_launcher.Checked = true;
            this.rb_launcher.ForeColor = System.Drawing.Color.White;
            this.rb_launcher.Location = new System.Drawing.Point(15, 68);
            this.rb_launcher.Name = "rb_launcher";
            this.rb_launcher.Size = new System.Drawing.Size(332, 24);
            this.rb_launcher.TabIndex = 6;
            this.rb_launcher.TabStop = true;
            this.rb_launcher.Text = "Launch a Game installed in Steam or Uplay";
            this.rb_launcher.UseVisualStyleBackColor = true;
            this.rb_launcher.CheckedChanged += new System.EventHandler(this.rb_launcher_CheckedChanged);
            // 
            // tabp_after
            // 
            this.tabp_after.BackColor = System.Drawing.Color.Black;
            this.tabp_after.Controls.Add(this.groupBox1);
            this.tabp_after.Controls.Add(this.gb_display_after);
            this.tabp_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_after.ForeColor = System.Drawing.Color.White;
            this.tabp_after.Location = new System.Drawing.Point(4, 32);
            this.tabp_after.Name = "tabp_after";
            this.tabp_after.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_after.Size = new System.Drawing.Size(1082, 594);
            this.tabp_after.TabIndex = 3;
            this.tabp_after.Text = "5. Choose what happens afterwards";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_switch_audio_permanent);
            this.groupBox1.Controls.Add(this.rb_switch_audio_temp);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(175, 311);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(765, 203);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "What happens to the Audio afterwards?";
            // 
            // rb_switch_audio_permanent
            // 
            this.rb_switch_audio_permanent.AutoSize = true;
            this.rb_switch_audio_permanent.ForeColor = System.Drawing.Color.White;
            this.rb_switch_audio_permanent.Location = new System.Drawing.Point(98, 116);
            this.rb_switch_audio_permanent.Name = "rb_switch_audio_permanent";
            this.rb_switch_audio_permanent.Size = new System.Drawing.Size(502, 24);
            this.rb_switch_audio_permanent.TabIndex = 12;
            this.rb_switch_audio_permanent.Text = "Keep using the Audio Device after Game ends (permanent change)";
            this.rb_switch_audio_permanent.UseVisualStyleBackColor = true;
            // 
            // rb_switch_audio_temp
            // 
            this.rb_switch_audio_temp.AutoSize = true;
            this.rb_switch_audio_temp.Checked = true;
            this.rb_switch_audio_temp.ForeColor = System.Drawing.Color.White;
            this.rb_switch_audio_temp.Location = new System.Drawing.Point(98, 68);
            this.rb_switch_audio_temp.Name = "rb_switch_audio_temp";
            this.rb_switch_audio_temp.Size = new System.Drawing.Size(563, 24);
            this.rb_switch_audio_temp.TabIndex = 11;
            this.rb_switch_audio_temp.TabStop = true;
            this.rb_switch_audio_temp.Text = "Revert back to original Audio Device (temporary change while running game)";
            this.rb_switch_audio_temp.UseVisualStyleBackColor = true;
            // 
            // gb_display_after
            // 
            this.gb_display_after.Controls.Add(this.rb_switch_display_permanent);
            this.gb_display_after.Controls.Add(this.rb_switch_display_temp);
            this.gb_display_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_display_after.ForeColor = System.Drawing.Color.White;
            this.gb_display_after.Location = new System.Drawing.Point(175, 58);
            this.gb_display_after.Name = "gb_display_after";
            this.gb_display_after.Size = new System.Drawing.Size(765, 203);
            this.gb_display_after.TabIndex = 11;
            this.gb_display_after.TabStop = false;
            this.gb_display_after.Text = "What happens to the Display Profile afterwards?";
            // 
            // rb_switch_display_permanent
            // 
            this.rb_switch_display_permanent.AutoSize = true;
            this.rb_switch_display_permanent.ForeColor = System.Drawing.Color.White;
            this.rb_switch_display_permanent.Location = new System.Drawing.Point(98, 116);
            this.rb_switch_display_permanent.Name = "rb_switch_display_permanent";
            this.rb_switch_display_permanent.Size = new System.Drawing.Size(508, 24);
            this.rb_switch_display_permanent.TabIndex = 12;
            this.rb_switch_display_permanent.Text = "Keep using the Display Profile after Game ends (permanent change)";
            this.rb_switch_display_permanent.UseVisualStyleBackColor = true;
            // 
            // rb_switch_display_temp
            // 
            this.rb_switch_display_temp.AutoSize = true;
            this.rb_switch_display_temp.Checked = true;
            this.rb_switch_display_temp.ForeColor = System.Drawing.Color.White;
            this.rb_switch_display_temp.Location = new System.Drawing.Point(98, 68);
            this.rb_switch_display_temp.Name = "rb_switch_display_temp";
            this.rb_switch_display_temp.Size = new System.Drawing.Size(569, 24);
            this.rb_switch_display_temp.TabIndex = 11;
            this.rb_switch_display_temp.TabStop = true;
            this.rb_switch_display_temp.Text = "Revert back to original Display Profile (temporary change while running game)";
            this.rb_switch_display_temp.UseVisualStyleBackColor = true;
            // 
            // txt_shortcut_save_name
            // 
            this.txt_shortcut_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_shortcut_save_name.Location = new System.Drawing.Point(326, 707);
            this.txt_shortcut_save_name.MaxLength = 200;
            this.txt_shortcut_save_name.Name = "txt_shortcut_save_name";
            this.txt_shortcut_save_name.Size = new System.Drawing.Size(511, 35);
            this.txt_shortcut_save_name.TabIndex = 29;
            this.txt_shortcut_save_name.Click += new System.EventHandler(this.txt_shortcut_save_name_Click);
            this.txt_shortcut_save_name.TextChanged += new System.EventHandler(this.txt_shortcut_save_name_TextChanged);
            // 
            // lbl_title
            // 
            this.lbl_title.AutoSize = true;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(385, 14);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(345, 33);
            this.lbl_title.TabIndex = 30;
            this.lbl_title.Text = "Configure Game Shortcut";
            // 
            // lbl_shortcut_name
            // 
            this.lbl_shortcut_name.AutoSize = true;
            this.lbl_shortcut_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_shortcut_name.ForeColor = System.Drawing.Color.Transparent;
            this.lbl_shortcut_name.Location = new System.Drawing.Point(142, 710);
            this.lbl_shortcut_name.Name = "lbl_shortcut_name";
            this.lbl_shortcut_name.Size = new System.Drawing.Size(178, 29);
            this.lbl_shortcut_name.TabIndex = 31;
            this.lbl_shortcut_name.Text = "Shortcut Name:";
            // 
            // cb_autosuggest
            // 
            this.cb_autosuggest.AutoSize = true;
            this.cb_autosuggest.Checked = true;
            this.cb_autosuggest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_autosuggest.ForeColor = System.Drawing.Color.White;
            this.cb_autosuggest.Location = new System.Drawing.Point(856, 716);
            this.cb_autosuggest.Name = "cb_autosuggest";
            this.cb_autosuggest.Size = new System.Drawing.Size(117, 17);
            this.cb_autosuggest.TabIndex = 32;
            this.cb_autosuggest.Text = "Auto-suggest name";
            this.cb_autosuggest.UseVisualStyleBackColor = true;
            this.cb_autosuggest.CheckedChanged += new System.EventHandler(this.cb_autosuggest_CheckedChanged);
            // 
            // ShortcutForm
            // 
            this.AcceptButton = this.btn_save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(1114, 806);
            this.Controls.Add(this.cb_autosuggest);
            this.Controls.Add(this.txt_shortcut_save_name);
            this.Controls.Add(this.lbl_shortcut_name);
            this.Controls.Add(this.lbl_title);
            this.Controls.Add(this.tabc_shortcut);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_save);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShortcutForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DisplayMagician - Configure a Game Shortcut";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShortcutForm_FormClosing);
            this.Load += new System.EventHandler(this.ShortcutForm_Load);
            this.tabc_shortcut.ResumeLayout(false);
            this.tabp_display.ResumeLayout(false);
            this.tabp_display.PerformLayout();
            this.tabp_audio.ResumeLayout(false);
            this.tabp_audio.PerformLayout();
            this.gb_audio_volume.ResumeLayout(false);
            this.gb_audio_volume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_audio_volume)).EndInit();
            this.tabp_before.ResumeLayout(false);
            this.pnl_start_program4.ResumeLayout(false);
            this.pnl_start_program4.PerformLayout();
            this.pnl_start_program3.ResumeLayout(false);
            this.pnl_start_program3.PerformLayout();
            this.pnl_start_program2.ResumeLayout(false);
            this.pnl_start_program2.PerformLayout();
            this.pnl_start_program1.ResumeLayout(false);
            this.pnl_start_program1.PerformLayout();
            this.tabp_game.ResumeLayout(false);
            this.tabp_game.PerformLayout();
            this.p_standalone.ResumeLayout(false);
            this.p_standalone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_executable)).EndInit();
            this.p_game.ResumeLayout(false);
            this.p_game.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_game)).EndInit();
            this.tabp_after.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gb_display_after.ResumeLayout(false);
            this.gb_display_after.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.OpenFileDialog dialog_open;
        private System.Windows.Forms.ImageList il_games;
        private System.Windows.Forms.TabControl tabc_shortcut;
        private System.Windows.Forms.TabPage tabp_display;
        private System.Windows.Forms.Label lbl_profile_shown_subtitle;
        private System.Windows.Forms.Label lbl_profile_shown;
        private Manina.Windows.Forms.ImageListView ilv_saved_profiles;
        private DisplayView dv_profile;
        private System.Windows.Forms.TabPage tabp_before;
        private System.Windows.Forms.TabPage tabp_game;
        private System.Windows.Forms.TabPage tabp_after;
        private System.Windows.Forms.TextBox txt_shortcut_save_name;
        private System.Windows.Forms.Panel p_game;
        private System.Windows.Forms.TextBox txt_game_launcher;
        private System.Windows.Forms.TextBox txt_game_name;
        private System.Windows.Forms.Label lbl_game_library;
        private System.Windows.Forms.Label lbl_game_name;
        private System.Windows.Forms.Button btn_choose_game;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_args_game;
        private System.Windows.Forms.CheckBox cb_args_game;
        private System.Windows.Forms.Label lbl_game_timeout;
        private System.Windows.Forms.NumericUpDown nud_timeout_game;
        private System.Windows.Forms.ListView lv_games;
        private System.Windows.Forms.ColumnHeader clm_images;
        private System.Windows.Forms.ColumnHeader clm_name;
        private System.Windows.Forms.RadioButton rb_launcher;
        private System.Windows.Forms.RadioButton rb_no_game;
        private System.Windows.Forms.Panel p_standalone;
        private System.Windows.Forms.TextBox txt_args_executable;
        private System.Windows.Forms.CheckBox cb_args_executable;
        private System.Windows.Forms.Button btn_app_different_executable;
        private System.Windows.Forms.TextBox txt_alternative_executable;
        private System.Windows.Forms.RadioButton rb_wait_alternative_executable;
        private System.Windows.Forms.RadioButton rb_wait_executable;
        private System.Windows.Forms.TextBox txt_executable;
        private System.Windows.Forms.Label lbl_app_executable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nud_timeout_executable;
        private System.Windows.Forms.RadioButton rb_standalone;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_shortcut_name;
        private System.Windows.Forms.Button btn_exe_to_start;
        private System.Windows.Forms.CheckBox cb_autosuggest;
        private System.Windows.Forms.Panel pnl_start_program1;
        private System.Windows.Forms.Label lbl_start_program1;
        private System.Windows.Forms.Panel pnl_start_program4;
        private System.Windows.Forms.TextBox txt_start_program4;
        private System.Windows.Forms.CheckBox cb_start_program_close4;
        private System.Windows.Forms.Button btn_start_program4;
        private System.Windows.Forms.TextBox txt_start_program_args4;
        private System.Windows.Forms.CheckBox cb_start_program_pass_args4;
        private System.Windows.Forms.Label lbl_start_program4;
        private System.Windows.Forms.Panel pnl_start_program3;
        private System.Windows.Forms.TextBox txt_start_program3;
        private System.Windows.Forms.CheckBox cb_start_program_close3;
        private System.Windows.Forms.Button btn_start_program3;
        private System.Windows.Forms.TextBox txt_start_program_args3;
        private System.Windows.Forms.CheckBox cb_start_program_pass_args3;
        private System.Windows.Forms.Label lbl_start_program3;
        private System.Windows.Forms.Panel pnl_start_program2;
        private System.Windows.Forms.TextBox txt_start_program2;
        private System.Windows.Forms.CheckBox cb_start_program_close2;
        private System.Windows.Forms.Button btn_start_program2;
        private System.Windows.Forms.TextBox txt_start_program_args2;
        private System.Windows.Forms.CheckBox cb_start_program_pass_args2;
        private System.Windows.Forms.Label lbl_start_program2;
        private System.Windows.Forms.TextBox txt_start_program1;
        private System.Windows.Forms.CheckBox cb_start_program_close1;
        private System.Windows.Forms.Button btn_start_program1;
        private System.Windows.Forms.TextBox txt_start_program_args1;
        private System.Windows.Forms.CheckBox cb_start_program_pass_args1;
        private System.Windows.Forms.CheckBox cb_start_program4;
        private System.Windows.Forms.CheckBox cb_start_program3;
        private System.Windows.Forms.CheckBox cb_start_program2;
        private System.Windows.Forms.CheckBox cb_start_program1;
        private System.Windows.Forms.TabPage tabp_audio;
        private System.Windows.Forms.RadioButton rb_no_change_audio;
        private System.Windows.Forms.RadioButton rb_change_audio;
        private System.Windows.Forms.ComboBox cb_audio_device;
        private System.Windows.Forms.Button btn_rescan_audio;
        private System.Windows.Forms.GroupBox gb_display_after;
        private System.Windows.Forms.RadioButton rb_switch_display_permanent;
        private System.Windows.Forms.RadioButton rb_switch_display_temp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_switch_audio_permanent;
        private System.Windows.Forms.RadioButton rb_switch_audio_temp;
        private System.Windows.Forms.GroupBox gb_audio_volume;
        private System.Windows.Forms.RadioButton rb_set_audio_volume;
        private System.Windows.Forms.RadioButton rb_keep_audio_volume;
        private System.Windows.Forms.Label lbl_audio_volume;
        private System.Windows.Forms.NumericUpDown nud_audio_volume;
    }
}