﻿using DisplayMagician.Resources;
using DisplayMagicianShared.UserControls;

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
            this.dv_profile = new DisplayMagicianShared.UserControls.DisplayView();
            this.tabp_audio = new System.Windows.Forms.TabPage();
            this.lbl_no_active_capture_devices = new System.Windows.Forms.Label();
            this.lbl_no_active_audio_devices = new System.Windows.Forms.Label();
            this.lbl_disabled_shortcut_audio_chipset = new System.Windows.Forms.Label();
            this.gb_capture_settings = new System.Windows.Forms.GroupBox();
            this.gb_capture_volume = new System.Windows.Forms.GroupBox();
            this.rb_set_capture_volume = new System.Windows.Forms.RadioButton();
            this.rb_keep_capture_volume = new System.Windows.Forms.RadioButton();
            this.lbl_capture_volume = new System.Windows.Forms.Label();
            this.nud_capture_volume = new System.Windows.Forms.NumericUpDown();
            this.btn_rescan_capture = new System.Windows.Forms.Button();
            this.cb_capture_device = new System.Windows.Forms.ComboBox();
            this.rb_change_capture = new System.Windows.Forms.RadioButton();
            this.rb_no_change_capture = new System.Windows.Forms.RadioButton();
            this.gb_audio_settings = new System.Windows.Forms.GroupBox();
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
            this.label3 = new System.Windows.Forms.Label();
            this.btn_add_new_start_program = new System.Windows.Forms.Button();
            this.flp_start_programs = new System.Windows.Forms.FlowLayoutPanel();
            this.tabp_game = new System.Windows.Forms.TabPage();
            this.lbl_no_game_libraries = new System.Windows.Forms.Label();
            this.p_standalone = new System.Windows.Forms.Panel();
            this.btn_exe_to_start = new System.Windows.Forms.Button();
            this.txt_args_executable = new System.Windows.Forms.TextBox();
            this.cb_args_executable = new System.Windows.Forms.CheckBox();
            this.btn_choose_alternative_executable = new System.Windows.Forms.Button();
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
            this.ilv_games = new Manina.Windows.Forms.ImageListView();
            this.cb_wait_alternative_game = new System.Windows.Forms.CheckBox();
            this.btn_choose_alternative_game = new System.Windows.Forms.Button();
            this.txt_alternative_game = new System.Windows.Forms.TextBox();
            this.txt_game_launcher = new System.Windows.Forms.TextBox();
            this.txt_game_name = new System.Windows.Forms.TextBox();
            this.lbl_game_library = new System.Windows.Forms.Label();
            this.lbl_game_name = new System.Windows.Forms.Label();
            this.txt_args_game = new System.Windows.Forms.TextBox();
            this.cb_args_game = new System.Windows.Forms.CheckBox();
            this.lbl_game_timeout = new System.Windows.Forms.Label();
            this.nud_timeout_game = new System.Windows.Forms.NumericUpDown();
            this.rb_launcher = new System.Windows.Forms.RadioButton();
            this.tabp_after = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rb_switch_capture_permanent = new System.Windows.Forms.RadioButton();
            this.rb_switch_capture_temp = new System.Windows.Forms.RadioButton();
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
            this.btn_hotkey = new System.Windows.Forms.Button();
            this.lbl_hotkey_assigned = new System.Windows.Forms.Label();
            this.btn_find_examples = new System.Windows.Forms.Button();
            this.tabc_shortcut.SuspendLayout();
            this.tabp_display.SuspendLayout();
            this.tabp_audio.SuspendLayout();
            this.gb_capture_settings.SuspendLayout();
            this.gb_capture_volume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_capture_volume)).BeginInit();
            this.gb_audio_settings.SuspendLayout();
            this.gb_audio_volume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_audio_volume)).BeginInit();
            this.tabp_before.SuspendLayout();
            this.tabp_game.SuspendLayout();
            this.p_standalone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_executable)).BeginInit();
            this.p_game.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_game)).BeginInit();
            this.tabp_after.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gb_display_after.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.Enabled = false;
            this.btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(560, 778);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(120, 40);
            this.btn_save.TabIndex = 6;
            this.btn_save.Text = "&Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_cancel.ForeColor = System.Drawing.Color.White;
            this.btn_cancel.Location = new System.Drawing.Point(1008, 793);
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
            this.tabc_shortcut.Size = new System.Drawing.Size(1090, 654);
            this.tabc_shortcut.TabIndex = 28;
            this.tabc_shortcut.Click += new System.EventHandler(this.tabc_shortcut_VisibleChanged);
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
            this.tabp_display.Size = new System.Drawing.Size(1082, 618);
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
            this.lbl_profile_shown_subtitle.Size = new System.Drawing.Size(397, 20);
            this.lbl_profile_shown_subtitle.TabIndex = 26;
            this.lbl_profile_shown_subtitle.Text = "Please select a Display Profile to use with this Shortcut.";
            // 
            // lbl_profile_shown
            // 
            this.lbl_profile_shown.AutoSize = true;
            this.lbl_profile_shown.BackColor = System.Drawing.Color.DimGray;
            this.lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown.Location = new System.Drawing.Point(18, 20);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(308, 29);
            this.lbl_profile_shown.TabIndex = 25;
            this.lbl_profile_shown.Text = "No Display Profile Selected";
            // 
            // ilv_saved_profiles
            // 
            this.ilv_saved_profiles.AllowCheckBoxClick = false;
            this.ilv_saved_profiles.AllowColumnClick = false;
            this.ilv_saved_profiles.AllowColumnResize = false;
            this.ilv_saved_profiles.AllowItemReorder = false;
            this.ilv_saved_profiles.AllowPaneResize = false;
            this.ilv_saved_profiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ilv_saved_profiles.Location = new System.Drawing.Point(0, 466);
            this.ilv_saved_profiles.MultiSelect = false;
            this.ilv_saved_profiles.Name = "ilv_saved_profiles";
            this.ilv_saved_profiles.PersistentCacheDirectory = "";
            this.ilv_saved_profiles.PersistentCacheSize = ((long)(100));
            this.ilv_saved_profiles.Size = new System.Drawing.Size(1086, 156);
            this.ilv_saved_profiles.TabIndex = 24;
            this.ilv_saved_profiles.UseWIC = true;
            this.ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            this.ilv_saved_profiles.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_saved_profiles_ItemClick);
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.tabp_audio.Controls.Add(this.lbl_no_active_capture_devices);
            this.tabp_audio.Controls.Add(this.lbl_no_active_audio_devices);
            this.tabp_audio.Controls.Add(this.lbl_disabled_shortcut_audio_chipset);
            this.tabp_audio.Controls.Add(this.gb_capture_settings);
            this.tabp_audio.Controls.Add(this.gb_audio_settings);
            this.tabp_audio.Location = new System.Drawing.Point(4, 32);
            this.tabp_audio.Name = "tabp_audio";
            this.tabp_audio.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_audio.Size = new System.Drawing.Size(1082, 618);
            this.tabp_audio.TabIndex = 4;
            this.tabp_audio.Text = "2. Choose Audio";
            // 
            // lbl_no_active_capture_devices
            // 
            this.lbl_no_active_capture_devices.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lbl_no_active_capture_devices.AutoSize = true;
            this.lbl_no_active_capture_devices.BackColor = System.Drawing.Color.Brown;
            this.lbl_no_active_capture_devices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_no_active_capture_devices.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_no_active_capture_devices.ForeColor = System.Drawing.Color.White;
            this.lbl_no_active_capture_devices.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_no_active_capture_devices.Location = new System.Drawing.Point(126, 433);
            this.lbl_no_active_capture_devices.Name = "lbl_no_active_capture_devices";
            this.lbl_no_active_capture_devices.Size = new System.Drawing.Size(831, 22);
            this.lbl_no_active_capture_devices.TabIndex = 36;
            this.lbl_no_active_capture_devices.Text = "No active microphone inputs found. Please connect or enable at least one micropho" +
    "ne if you want to use this feature.";
            this.lbl_no_active_capture_devices.Visible = false;
            // 
            // lbl_no_active_audio_devices
            // 
            this.lbl_no_active_audio_devices.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_no_active_audio_devices.AutoSize = true;
            this.lbl_no_active_audio_devices.BackColor = System.Drawing.Color.Brown;
            this.lbl_no_active_audio_devices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_no_active_audio_devices.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_no_active_audio_devices.ForeColor = System.Drawing.Color.White;
            this.lbl_no_active_audio_devices.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_no_active_audio_devices.Location = new System.Drawing.Point(131, 151);
            this.lbl_no_active_audio_devices.Name = "lbl_no_active_audio_devices";
            this.lbl_no_active_audio_devices.Size = new System.Drawing.Size(804, 22);
            this.lbl_no_active_audio_devices.TabIndex = 35;
            this.lbl_no_active_audio_devices.Text = "No active audio outputs found. Please connect or enable at least one audio output" +
    " if you want to use this feature.";
            this.lbl_no_active_audio_devices.Visible = false;
            // 
            // lbl_disabled_shortcut_audio_chipset
            // 
            this.lbl_disabled_shortcut_audio_chipset.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lbl_disabled_shortcut_audio_chipset.AutoSize = true;
            this.lbl_disabled_shortcut_audio_chipset.BackColor = System.Drawing.Color.Brown;
            this.lbl_disabled_shortcut_audio_chipset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_disabled_shortcut_audio_chipset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_disabled_shortcut_audio_chipset.ForeColor = System.Drawing.Color.White;
            this.lbl_disabled_shortcut_audio_chipset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_disabled_shortcut_audio_chipset.Location = new System.Drawing.Point(263, 298);
            this.lbl_disabled_shortcut_audio_chipset.Name = "lbl_disabled_shortcut_audio_chipset";
            this.lbl_disabled_shortcut_audio_chipset.Size = new System.Drawing.Size(557, 22);
            this.lbl_disabled_shortcut_audio_chipset.TabIndex = 34;
            this.lbl_disabled_shortcut_audio_chipset.Text = "Unsupported Audio Chipset. Setting audio isn\'t supported on your computer :(";
            this.lbl_disabled_shortcut_audio_chipset.Visible = false;
            // 
            // gb_capture_settings
            // 
            this.gb_capture_settings.Controls.Add(this.gb_capture_volume);
            this.gb_capture_settings.Controls.Add(this.btn_rescan_capture);
            this.gb_capture_settings.Controls.Add(this.cb_capture_device);
            this.gb_capture_settings.Controls.Add(this.rb_change_capture);
            this.gb_capture_settings.Controls.Add(this.rb_no_change_capture);
            this.gb_capture_settings.ForeColor = System.Drawing.Color.White;
            this.gb_capture_settings.Location = new System.Drawing.Point(65, 317);
            this.gb_capture_settings.Name = "gb_capture_settings";
            this.gb_capture_settings.Size = new System.Drawing.Size(953, 256);
            this.gb_capture_settings.TabIndex = 21;
            this.gb_capture_settings.TabStop = false;
            this.gb_capture_settings.Text = "Microphone Settings";
            // 
            // gb_capture_volume
            // 
            this.gb_capture_volume.Controls.Add(this.rb_set_capture_volume);
            this.gb_capture_volume.Controls.Add(this.rb_keep_capture_volume);
            this.gb_capture_volume.Controls.Add(this.lbl_capture_volume);
            this.gb_capture_volume.Controls.Add(this.nud_capture_volume);
            this.gb_capture_volume.ForeColor = System.Drawing.Color.White;
            this.gb_capture_volume.Location = new System.Drawing.Point(327, 114);
            this.gb_capture_volume.Name = "gb_capture_volume";
            this.gb_capture_volume.Size = new System.Drawing.Size(429, 128);
            this.gb_capture_volume.TabIndex = 20;
            this.gb_capture_volume.TabStop = false;
            this.gb_capture_volume.Text = "Microphone Volume";
            this.gb_capture_volume.Visible = false;
            // 
            // rb_set_capture_volume
            // 
            this.rb_set_capture_volume.AutoSize = true;
            this.rb_set_capture_volume.ForeColor = System.Drawing.Color.White;
            this.rb_set_capture_volume.Location = new System.Drawing.Point(62, 78);
            this.rb_set_capture_volume.Name = "rb_set_capture_volume";
            this.rb_set_capture_volume.Size = new System.Drawing.Size(167, 24);
            this.rb_set_capture_volume.TabIndex = 13;
            this.rb_set_capture_volume.Text = "Set audio volume at";
            this.rb_set_capture_volume.UseVisualStyleBackColor = true;
            this.rb_set_capture_volume.CheckedChanged += new System.EventHandler(this.rb_set_capture_volume_CheckedChanged);
            // 
            // rb_keep_capture_volume
            // 
            this.rb_keep_capture_volume.AutoSize = true;
            this.rb_keep_capture_volume.Checked = true;
            this.rb_keep_capture_volume.ForeColor = System.Drawing.Color.White;
            this.rb_keep_capture_volume.Location = new System.Drawing.Point(62, 35);
            this.rb_keep_capture_volume.Name = "rb_keep_capture_volume";
            this.rb_keep_capture_volume.Size = new System.Drawing.Size(203, 24);
            this.rb_keep_capture_volume.TabIndex = 12;
            this.rb_keep_capture_volume.TabStop = true;
            this.rb_keep_capture_volume.Text = "Leave audio volume as is";
            this.rb_keep_capture_volume.UseVisualStyleBackColor = true;
            this.rb_keep_capture_volume.CheckedChanged += new System.EventHandler(this.rb_keep_capture_volume_CheckedChanged);
            // 
            // lbl_capture_volume
            // 
            this.lbl_capture_volume.AutoSize = true;
            this.lbl_capture_volume.ForeColor = System.Drawing.Color.White;
            this.lbl_capture_volume.Location = new System.Drawing.Point(299, 80);
            this.lbl_capture_volume.Name = "lbl_capture_volume";
            this.lbl_capture_volume.Size = new System.Drawing.Size(63, 20);
            this.lbl_capture_volume.TabIndex = 11;
            this.lbl_capture_volume.Text = "percent";
            // 
            // nud_capture_volume
            // 
            this.nud_capture_volume.Enabled = false;
            this.nud_capture_volume.Location = new System.Drawing.Point(233, 78);
            this.nud_capture_volume.Name = "nud_capture_volume";
            this.nud_capture_volume.Size = new System.Drawing.Size(60, 26);
            this.nud_capture_volume.TabIndex = 10;
            this.nud_capture_volume.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_capture_volume.ValueChanged += new System.EventHandler(this.nud_capture_volume_ValueChanged);
            // 
            // btn_rescan_capture
            // 
            this.btn_rescan_capture.Enabled = false;
            this.btn_rescan_capture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_rescan_capture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_rescan_capture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rescan_capture.ForeColor = System.Drawing.Color.White;
            this.btn_rescan_capture.Location = new System.Drawing.Point(760, 73);
            this.btn_rescan_capture.Name = "btn_rescan_capture";
            this.btn_rescan_capture.Size = new System.Drawing.Size(71, 28);
            this.btn_rescan_capture.TabIndex = 19;
            this.btn_rescan_capture.Text = "rescan";
            this.btn_rescan_capture.UseVisualStyleBackColor = true;
            this.btn_rescan_capture.Click += new System.EventHandler(this.btn_rescan_capture_Click);
            // 
            // cb_capture_device
            // 
            this.cb_capture_device.Enabled = false;
            this.cb_capture_device.FormattingEnabled = true;
            this.cb_capture_device.Location = new System.Drawing.Point(325, 73);
            this.cb_capture_device.Name = "cb_capture_device";
            this.cb_capture_device.Size = new System.Drawing.Size(429, 28);
            this.cb_capture_device.TabIndex = 18;
            this.cb_capture_device.SelectedIndexChanged += new System.EventHandler(this.cb_capture_device_SelectedIndexChanged);
            // 
            // rb_change_capture
            // 
            this.rb_change_capture.AutoSize = true;
            this.rb_change_capture.ForeColor = System.Drawing.Color.White;
            this.rb_change_capture.Location = new System.Drawing.Point(121, 73);
            this.rb_change_capture.Name = "rb_change_capture";
            this.rb_change_capture.Size = new System.Drawing.Size(192, 24);
            this.rb_change_capture.TabIndex = 17;
            this.rb_change_capture.Text = "Change microphone to:";
            this.rb_change_capture.UseVisualStyleBackColor = true;
            this.rb_change_capture.CheckedChanged += new System.EventHandler(this.rb_change_capture_CheckedChanged);
            // 
            // rb_no_change_capture
            // 
            this.rb_no_change_capture.AutoSize = true;
            this.rb_no_change_capture.Checked = true;
            this.rb_no_change_capture.ForeColor = System.Drawing.Color.White;
            this.rb_no_change_capture.Location = new System.Drawing.Point(121, 35);
            this.rb_no_change_capture.Name = "rb_no_change_capture";
            this.rb_no_change_capture.Size = new System.Drawing.Size(308, 24);
            this.rb_no_change_capture.TabIndex = 16;
            this.rb_no_change_capture.TabStop = true;
            this.rb_no_change_capture.Text = "Don\'t change microphone input settings";
            this.rb_no_change_capture.UseVisualStyleBackColor = true;
            this.rb_no_change_capture.CheckedChanged += new System.EventHandler(this.rb_no_change_capture_CheckedChanged);
            // 
            // gb_audio_settings
            // 
            this.gb_audio_settings.Controls.Add(this.gb_audio_volume);
            this.gb_audio_settings.Controls.Add(this.btn_rescan_audio);
            this.gb_audio_settings.Controls.Add(this.cb_audio_device);
            this.gb_audio_settings.Controls.Add(this.rb_change_audio);
            this.gb_audio_settings.Controls.Add(this.rb_no_change_audio);
            this.gb_audio_settings.ForeColor = System.Drawing.Color.White;
            this.gb_audio_settings.Location = new System.Drawing.Point(65, 30);
            this.gb_audio_settings.Name = "gb_audio_settings";
            this.gb_audio_settings.Size = new System.Drawing.Size(953, 272);
            this.gb_audio_settings.TabIndex = 0;
            this.gb_audio_settings.TabStop = false;
            this.gb_audio_settings.Text = "Audio Output Settings";
            // 
            // gb_audio_volume
            // 
            this.gb_audio_volume.Controls.Add(this.rb_set_audio_volume);
            this.gb_audio_volume.Controls.Add(this.rb_keep_audio_volume);
            this.gb_audio_volume.Controls.Add(this.lbl_audio_volume);
            this.gb_audio_volume.Controls.Add(this.nud_audio_volume);
            this.gb_audio_volume.ForeColor = System.Drawing.Color.White;
            this.gb_audio_volume.Location = new System.Drawing.Point(325, 113);
            this.gb_audio_volume.Name = "gb_audio_volume";
            this.gb_audio_volume.Size = new System.Drawing.Size(429, 133);
            this.gb_audio_volume.TabIndex = 20;
            this.gb_audio_volume.TabStop = false;
            this.gb_audio_volume.Text = "Audio Output Volume";
            this.gb_audio_volume.Visible = false;
            // 
            // rb_set_audio_volume
            // 
            this.rb_set_audio_volume.AutoSize = true;
            this.rb_set_audio_volume.ForeColor = System.Drawing.Color.White;
            this.rb_set_audio_volume.Location = new System.Drawing.Point(61, 82);
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
            this.rb_keep_audio_volume.Location = new System.Drawing.Point(61, 38);
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
            this.lbl_audio_volume.Location = new System.Drawing.Point(298, 84);
            this.lbl_audio_volume.Name = "lbl_audio_volume";
            this.lbl_audio_volume.Size = new System.Drawing.Size(63, 20);
            this.lbl_audio_volume.TabIndex = 11;
            this.lbl_audio_volume.Text = "percent";
            // 
            // nud_audio_volume
            // 
            this.nud_audio_volume.Enabled = false;
            this.nud_audio_volume.Location = new System.Drawing.Point(232, 82);
            this.nud_audio_volume.Name = "nud_audio_volume";
            this.nud_audio_volume.Size = new System.Drawing.Size(60, 26);
            this.nud_audio_volume.TabIndex = 10;
            this.nud_audio_volume.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_audio_volume.ValueChanged += new System.EventHandler(this.nud_audio_volume_ValueChanged);
            // 
            // btn_rescan_audio
            // 
            this.btn_rescan_audio.Enabled = false;
            this.btn_rescan_audio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_rescan_audio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_rescan_audio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rescan_audio.ForeColor = System.Drawing.Color.White;
            this.btn_rescan_audio.Location = new System.Drawing.Point(760, 72);
            this.btn_rescan_audio.Name = "btn_rescan_audio";
            this.btn_rescan_audio.Size = new System.Drawing.Size(71, 28);
            this.btn_rescan_audio.TabIndex = 19;
            this.btn_rescan_audio.Text = "rescan";
            this.btn_rescan_audio.UseVisualStyleBackColor = true;
            this.btn_rescan_audio.Click += new System.EventHandler(this.btn_rescan_audio_Click);
            // 
            // cb_audio_device
            // 
            this.cb_audio_device.Enabled = false;
            this.cb_audio_device.FormattingEnabled = true;
            this.cb_audio_device.Location = new System.Drawing.Point(325, 72);
            this.cb_audio_device.Name = "cb_audio_device";
            this.cb_audio_device.Size = new System.Drawing.Size(429, 28);
            this.cb_audio_device.TabIndex = 18;
            this.cb_audio_device.SelectedIndexChanged += new System.EventHandler(this.cb_audio_device_SelectedIndexChanged);
            // 
            // rb_change_audio
            // 
            this.rb_change_audio.AutoSize = true;
            this.rb_change_audio.ForeColor = System.Drawing.Color.White;
            this.rb_change_audio.Location = new System.Drawing.Point(121, 72);
            this.rb_change_audio.Name = "rb_change_audio";
            this.rb_change_audio.Size = new System.Drawing.Size(198, 24);
            this.rb_change_audio.TabIndex = 17;
            this.rb_change_audio.Text = "Change audio output to:";
            this.rb_change_audio.UseVisualStyleBackColor = true;
            this.rb_change_audio.CheckedChanged += new System.EventHandler(this.rb_change_audio_CheckedChanged);
            // 
            // rb_no_change_audio
            // 
            this.rb_no_change_audio.AutoSize = true;
            this.rb_no_change_audio.Checked = true;
            this.rb_no_change_audio.ForeColor = System.Drawing.Color.White;
            this.rb_no_change_audio.Location = new System.Drawing.Point(121, 34);
            this.rb_no_change_audio.Name = "rb_no_change_audio";
            this.rb_no_change_audio.Size = new System.Drawing.Size(275, 24);
            this.rb_no_change_audio.TabIndex = 16;
            this.rb_no_change_audio.TabStop = true;
            this.rb_no_change_audio.Text = "Don\'t change audio output settings";
            this.rb_no_change_audio.UseVisualStyleBackColor = true;
            this.rb_no_change_audio.CheckedChanged += new System.EventHandler(this.rb_no_change_audio_CheckedChanged);
            // 
            // tabp_before
            // 
            this.tabp_before.BackColor = System.Drawing.Color.Black;
            this.tabp_before.Controls.Add(this.btn_find_examples);
            this.tabp_before.Controls.Add(this.label3);
            this.tabp_before.Controls.Add(this.btn_add_new_start_program);
            this.tabp_before.Controls.Add(this.flp_start_programs);
            this.tabp_before.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_before.ForeColor = System.Drawing.Color.White;
            this.tabp_before.Location = new System.Drawing.Point(4, 32);
            this.tabp_before.Name = "tabp_before";
            this.tabp_before.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_before.Size = new System.Drawing.Size(1082, 618);
            this.tabp_before.TabIndex = 1;
            this.tabp_before.Text = "3. Choose what happens before";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(138, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(807, 20);
            this.label3.TabIndex = 39;
            this.label3.Text = "Add one or more additional programs to start before the main Game starts. They wi" +
    "ll start in the order listed below.";
            // 
            // btn_add_new_start_program
            // 
            this.btn_add_new_start_program.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_add_new_start_program.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_add_new_start_program.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_add_new_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_add_new_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_add_new_start_program.ForeColor = System.Drawing.Color.White;
            this.btn_add_new_start_program.Location = new System.Drawing.Point(424, 61);
            this.btn_add_new_start_program.Name = "btn_add_new_start_program";
            this.btn_add_new_start_program.Size = new System.Drawing.Size(235, 40);
            this.btn_add_new_start_program.TabIndex = 38;
            this.btn_add_new_start_program.Text = "&Add Start Program";
            this.btn_add_new_start_program.UseVisualStyleBackColor = true;
            this.btn_add_new_start_program.Click += new System.EventHandler(this.btn_add_new_start_program_Click);
            // 
            // flp_start_programs
            // 
            this.flp_start_programs.AllowDrop = true;
            this.flp_start_programs.AutoScroll = true;
            this.flp_start_programs.AutoScrollMargin = new System.Drawing.Size(5, 0);
            this.flp_start_programs.AutoScrollMinSize = new System.Drawing.Size(5, 0);
            this.flp_start_programs.BackColor = System.Drawing.Color.White;
            this.flp_start_programs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flp_start_programs.Location = new System.Drawing.Point(3, 107);
            this.flp_start_programs.Name = "flp_start_programs";
            this.flp_start_programs.Size = new System.Drawing.Size(1076, 508);
            this.flp_start_programs.TabIndex = 0;
            // 
            // tabp_game
            // 
            this.tabp_game.BackColor = System.Drawing.Color.Black;
            this.tabp_game.Controls.Add(this.lbl_no_game_libraries);
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
            this.tabp_game.Size = new System.Drawing.Size(1082, 618);
            this.tabp_game.TabIndex = 2;
            this.tabp_game.Text = "4. Choose Game to start";
            // 
            // lbl_no_game_libraries
            // 
            this.lbl_no_game_libraries.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_no_game_libraries.AutoSize = true;
            this.lbl_no_game_libraries.BackColor = System.Drawing.Color.Brown;
            this.lbl_no_game_libraries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_no_game_libraries.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_no_game_libraries.ForeColor = System.Drawing.Color.White;
            this.lbl_no_game_libraries.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_no_game_libraries.Location = new System.Drawing.Point(375, 262);
            this.lbl_no_game_libraries.Name = "lbl_no_game_libraries";
            this.lbl_no_game_libraries.Size = new System.Drawing.Size(280, 22);
            this.lbl_no_game_libraries.TabIndex = 34;
            this.lbl_no_game_libraries.Text = "No supported game libraries detected!";
            this.lbl_no_game_libraries.Visible = false;
            // 
            // p_standalone
            // 
            this.p_standalone.Controls.Add(this.btn_exe_to_start);
            this.p_standalone.Controls.Add(this.txt_args_executable);
            this.p_standalone.Controls.Add(this.cb_args_executable);
            this.p_standalone.Controls.Add(this.btn_choose_alternative_executable);
            this.p_standalone.Controls.Add(this.txt_alternative_executable);
            this.p_standalone.Controls.Add(this.rb_wait_alternative_executable);
            this.p_standalone.Controls.Add(this.rb_wait_executable);
            this.p_standalone.Controls.Add(this.txt_executable);
            this.p_standalone.Controls.Add(this.lbl_app_executable);
            this.p_standalone.Controls.Add(this.label2);
            this.p_standalone.Controls.Add(this.nud_timeout_executable);
            this.p_standalone.Enabled = false;
            this.p_standalone.Location = new System.Drawing.Point(35, 86);
            this.p_standalone.Name = "p_standalone";
            this.p_standalone.Size = new System.Drawing.Size(1006, 160);
            this.p_standalone.TabIndex = 10;
            // 
            // btn_exe_to_start
            // 
            this.btn_exe_to_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_exe_to_start.ForeColor = System.Drawing.Color.White;
            this.btn_exe_to_start.Location = new System.Drawing.Point(666, 10);
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
            // btn_choose_alternative_executable
            // 
            this.btn_choose_alternative_executable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_choose_alternative_executable.ForeColor = System.Drawing.Color.White;
            this.btn_choose_alternative_executable.Location = new System.Drawing.Point(877, 115);
            this.btn_choose_alternative_executable.Name = "btn_choose_alternative_executable";
            this.btn_choose_alternative_executable.Size = new System.Drawing.Size(85, 27);
            this.btn_choose_alternative_executable.TabIndex = 9;
            this.btn_choose_alternative_executable.Text = "Choose";
            this.btn_choose_alternative_executable.UseVisualStyleBackColor = true;
            this.btn_choose_alternative_executable.Click += new System.EventHandler(this.btn_choose_alternative_executable_Click);
            // 
            // txt_alternative_executable
            // 
            this.txt_alternative_executable.Enabled = false;
            this.txt_alternative_executable.Location = new System.Drawing.Point(493, 116);
            this.txt_alternative_executable.Name = "txt_alternative_executable";
            this.txt_alternative_executable.Size = new System.Drawing.Size(378, 26);
            this.txt_alternative_executable.TabIndex = 4;
            this.txt_alternative_executable.TextChanged += new System.EventHandler(this.txt_alternative_executable_TextChanged);
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
            this.rb_wait_alternative_executable.CheckedChanged += new System.EventHandler(this.rb_wait_alternative_executable_CheckedChanged);
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
            this.txt_executable.Size = new System.Drawing.Size(489, 26);
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
            this.label2.Location = new System.Drawing.Point(783, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Max Wait (secs):";
            this.label2.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // nud_timeout_executable
            // 
            this.nud_timeout_executable.Location = new System.Drawing.Point(910, 10);
            this.nud_timeout_executable.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_timeout_executable.Name = "nud_timeout_executable";
            this.nud_timeout_executable.Size = new System.Drawing.Size(55, 26);
            this.nud_timeout_executable.TabIndex = 6;
            this.nud_timeout_executable.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // rb_standalone
            // 
            this.rb_standalone.AutoSize = true;
            this.rb_standalone.ForeColor = System.Drawing.Color.White;
            this.rb_standalone.Location = new System.Drawing.Point(16, 60);
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
            this.p_game.Controls.Add(this.ilv_games);
            this.p_game.Controls.Add(this.cb_wait_alternative_game);
            this.p_game.Controls.Add(this.btn_choose_alternative_game);
            this.p_game.Controls.Add(this.txt_alternative_game);
            this.p_game.Controls.Add(this.txt_game_launcher);
            this.p_game.Controls.Add(this.txt_game_name);
            this.p_game.Controls.Add(this.lbl_game_library);
            this.p_game.Controls.Add(this.lbl_game_name);
            this.p_game.Controls.Add(this.txt_args_game);
            this.p_game.Controls.Add(this.cb_args_game);
            this.p_game.Controls.Add(this.lbl_game_timeout);
            this.p_game.Controls.Add(this.nud_timeout_game);
            this.p_game.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.p_game.Location = new System.Drawing.Point(3, 292);
            this.p_game.Name = "p_game";
            this.p_game.Size = new System.Drawing.Size(1076, 323);
            this.p_game.TabIndex = 7;
            // 
            // ilv_games
            // 
            this.ilv_games.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ilv_games.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ilv_games.IntegralScroll = true;
            this.ilv_games.Location = new System.Drawing.Point(0, 97);
            this.ilv_games.Name = "ilv_games";
            this.ilv_games.PersistentCacheDirectory = "";
            this.ilv_games.PersistentCacheSize = ((long)(100));
            this.ilv_games.Size = new System.Drawing.Size(1076, 226);
            this.ilv_games.SortOrder = Manina.Windows.Forms.SortOrder.Ascending;
            this.ilv_games.TabIndex = 28;
            this.ilv_games.UseWIC = true;
            this.ilv_games.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_games_ItemClick);
            // 
            // cb_wait_alternative_game
            // 
            this.cb_wait_alternative_game.AutoSize = true;
            this.cb_wait_alternative_game.Location = new System.Drawing.Point(555, 45);
            this.cb_wait_alternative_game.Name = "cb_wait_alternative_game";
            this.cb_wait_alternative_game.Size = new System.Drawing.Size(229, 24);
            this.cb_wait_alternative_game.TabIndex = 27;
            this.cb_wait_alternative_game.Text = "Monitor different executable:";
            this.cb_wait_alternative_game.UseVisualStyleBackColor = true;
            this.cb_wait_alternative_game.CheckedChanged += new System.EventHandler(this.cb_wait_alternative_game_CheckedChanged);
            // 
            // btn_choose_alternative_game
            // 
            this.btn_choose_alternative_game.Enabled = false;
            this.btn_choose_alternative_game.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_choose_alternative_game.ForeColor = System.Drawing.Color.White;
            this.btn_choose_alternative_game.Location = new System.Drawing.Point(983, 42);
            this.btn_choose_alternative_game.Name = "btn_choose_alternative_game";
            this.btn_choose_alternative_game.Size = new System.Drawing.Size(85, 27);
            this.btn_choose_alternative_game.TabIndex = 26;
            this.btn_choose_alternative_game.Text = "Choose";
            this.btn_choose_alternative_game.UseVisualStyleBackColor = true;
            this.btn_choose_alternative_game.Click += new System.EventHandler(this.btn_choose_alternative_game_Click);
            // 
            // txt_alternative_game
            // 
            this.txt_alternative_game.Enabled = false;
            this.txt_alternative_game.Location = new System.Drawing.Point(788, 43);
            this.txt_alternative_game.Name = "txt_alternative_game";
            this.txt_alternative_game.Size = new System.Drawing.Size(193, 26);
            this.txt_alternative_game.TabIndex = 24;
            // 
            // txt_game_launcher
            // 
            this.txt_game_launcher.Location = new System.Drawing.Point(150, 43);
            this.txt_game_launcher.Name = "txt_game_launcher";
            this.txt_game_launcher.ReadOnly = true;
            this.txt_game_launcher.Size = new System.Drawing.Size(149, 26);
            this.txt_game_launcher.TabIndex = 23;
            // 
            // txt_game_name
            // 
            this.txt_game_name.Location = new System.Drawing.Point(150, 11);
            this.txt_game_name.Name = "txt_game_name";
            this.txt_game_name.ReadOnly = true;
            this.txt_game_name.Size = new System.Drawing.Size(385, 26);
            this.txt_game_name.TabIndex = 21;
            // 
            // lbl_game_library
            // 
            this.lbl_game_library.AutoSize = true;
            this.lbl_game_library.ForeColor = System.Drawing.Color.White;
            this.lbl_game_library.Location = new System.Drawing.Point(36, 46);
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
            this.lbl_game_name.Location = new System.Drawing.Point(25, 14);
            this.lbl_game_name.Name = "lbl_game_name";
            this.lbl_game_name.Size = new System.Drawing.Size(124, 20);
            this.lbl_game_name.TabIndex = 17;
            this.lbl_game_name.Text = "Selected Game:";
            this.lbl_game_name.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_game_name.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // txt_args_game
            // 
            this.txt_args_game.Enabled = false;
            this.txt_args_game.Location = new System.Drawing.Point(788, 11);
            this.txt_args_game.Name = "txt_args_game";
            this.txt_args_game.Size = new System.Drawing.Size(279, 26);
            this.txt_args_game.TabIndex = 13;
            // 
            // cb_args_game
            // 
            this.cb_args_game.AutoSize = true;
            this.cb_args_game.ForeColor = System.Drawing.Color.White;
            this.cb_args_game.Location = new System.Drawing.Point(555, 13);
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
            this.lbl_game_timeout.Location = new System.Drawing.Point(349, 45);
            this.lbl_game_timeout.Name = "lbl_game_timeout";
            this.lbl_game_timeout.Size = new System.Drawing.Size(125, 20);
            this.lbl_game_timeout.TabIndex = 4;
            this.lbl_game_timeout.Text = "Max Wait (secs):";
            this.lbl_game_timeout.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
            // 
            // nud_timeout_game
            // 
            this.nud_timeout_game.Location = new System.Drawing.Point(480, 43);
            this.nud_timeout_game.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_timeout_game.Name = "nud_timeout_game";
            this.nud_timeout_game.Size = new System.Drawing.Size(54, 26);
            this.nud_timeout_game.TabIndex = 5;
            this.nud_timeout_game.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // rb_launcher
            // 
            this.rb_launcher.AutoSize = true;
            this.rb_launcher.Checked = true;
            this.rb_launcher.ForeColor = System.Drawing.Color.White;
            this.rb_launcher.Location = new System.Drawing.Point(15, 262);
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
            this.tabp_after.Controls.Add(this.groupBox2);
            this.tabp_after.Controls.Add(this.groupBox1);
            this.tabp_after.Controls.Add(this.gb_display_after);
            this.tabp_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabp_after.ForeColor = System.Drawing.Color.White;
            this.tabp_after.Location = new System.Drawing.Point(4, 32);
            this.tabp_after.Name = "tabp_after";
            this.tabp_after.Padding = new System.Windows.Forms.Padding(3);
            this.tabp_after.Size = new System.Drawing.Size(1082, 618);
            this.tabp_after.TabIndex = 3;
            this.tabp_after.Text = "5. Choose what happens afterwards";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rb_switch_capture_permanent);
            this.groupBox2.Controls.Add(this.rb_switch_capture_temp);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(175, 404);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(765, 161);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "What happens to the Microphone afterwards?";
            // 
            // rb_switch_capture_permanent
            // 
            this.rb_switch_capture_permanent.AutoSize = true;
            this.rb_switch_capture_permanent.ForeColor = System.Drawing.Color.White;
            this.rb_switch_capture_permanent.Location = new System.Drawing.Point(98, 96);
            this.rb_switch_capture_permanent.Name = "rb_switch_capture_permanent";
            this.rb_switch_capture_permanent.Size = new System.Drawing.Size(492, 24);
            this.rb_switch_capture_permanent.TabIndex = 12;
            this.rb_switch_capture_permanent.Text = "Keep using the Microphone after Game ends (permanent change)";
            this.rb_switch_capture_permanent.UseVisualStyleBackColor = true;
            this.rb_switch_capture_permanent.CheckedChanged += new System.EventHandler(this.rb_switch_capture_permanent_CheckedChanged);
            // 
            // rb_switch_capture_temp
            // 
            this.rb_switch_capture_temp.AutoSize = true;
            this.rb_switch_capture_temp.Checked = true;
            this.rb_switch_capture_temp.ForeColor = System.Drawing.Color.White;
            this.rb_switch_capture_temp.Location = new System.Drawing.Point(98, 48);
            this.rb_switch_capture_temp.Name = "rb_switch_capture_temp";
            this.rb_switch_capture_temp.Size = new System.Drawing.Size(553, 24);
            this.rb_switch_capture_temp.TabIndex = 11;
            this.rb_switch_capture_temp.TabStop = true;
            this.rb_switch_capture_temp.Text = "Revert back to original Microphone (temporary change while running game)";
            this.rb_switch_capture_temp.UseVisualStyleBackColor = true;
            this.rb_switch_capture_temp.CheckedChanged += new System.EventHandler(this.rb_switch_capture_temp_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_switch_audio_permanent);
            this.groupBox1.Controls.Add(this.rb_switch_audio_temp);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(175, 219);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(765, 161);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "What happens to the Audio output afterwards?";
            // 
            // rb_switch_audio_permanent
            // 
            this.rb_switch_audio_permanent.AutoSize = true;
            this.rb_switch_audio_permanent.ForeColor = System.Drawing.Color.White;
            this.rb_switch_audio_permanent.Location = new System.Drawing.Point(98, 96);
            this.rb_switch_audio_permanent.Name = "rb_switch_audio_permanent";
            this.rb_switch_audio_permanent.Size = new System.Drawing.Size(502, 24);
            this.rb_switch_audio_permanent.TabIndex = 12;
            this.rb_switch_audio_permanent.Text = "Keep using the Audio Device after Game ends (permanent change)";
            this.rb_switch_audio_permanent.UseVisualStyleBackColor = true;
            this.rb_switch_audio_permanent.CheckedChanged += new System.EventHandler(this.rb_switch_audio_permanent_CheckedChanged);
            // 
            // rb_switch_audio_temp
            // 
            this.rb_switch_audio_temp.AutoSize = true;
            this.rb_switch_audio_temp.Checked = true;
            this.rb_switch_audio_temp.ForeColor = System.Drawing.Color.White;
            this.rb_switch_audio_temp.Location = new System.Drawing.Point(98, 48);
            this.rb_switch_audio_temp.Name = "rb_switch_audio_temp";
            this.rb_switch_audio_temp.Size = new System.Drawing.Size(563, 24);
            this.rb_switch_audio_temp.TabIndex = 11;
            this.rb_switch_audio_temp.TabStop = true;
            this.rb_switch_audio_temp.Text = "Revert back to original Audio Device (temporary change while running game)";
            this.rb_switch_audio_temp.UseVisualStyleBackColor = true;
            this.rb_switch_audio_temp.CheckedChanged += new System.EventHandler(this.rb_switch_audio_temp_CheckedChanged);
            // 
            // gb_display_after
            // 
            this.gb_display_after.Controls.Add(this.rb_switch_display_permanent);
            this.gb_display_after.Controls.Add(this.rb_switch_display_temp);
            this.gb_display_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_display_after.ForeColor = System.Drawing.Color.White;
            this.gb_display_after.Location = new System.Drawing.Point(175, 31);
            this.gb_display_after.Name = "gb_display_after";
            this.gb_display_after.Size = new System.Drawing.Size(765, 162);
            this.gb_display_after.TabIndex = 11;
            this.gb_display_after.TabStop = false;
            this.gb_display_after.Text = "What happens to the Display Profile afterwards?";
            // 
            // rb_switch_display_permanent
            // 
            this.rb_switch_display_permanent.AutoSize = true;
            this.rb_switch_display_permanent.ForeColor = System.Drawing.Color.White;
            this.rb_switch_display_permanent.Location = new System.Drawing.Point(98, 96);
            this.rb_switch_display_permanent.Name = "rb_switch_display_permanent";
            this.rb_switch_display_permanent.Size = new System.Drawing.Size(508, 24);
            this.rb_switch_display_permanent.TabIndex = 12;
            this.rb_switch_display_permanent.Text = "Keep using the Display Profile after Game ends (permanent change)";
            this.rb_switch_display_permanent.UseVisualStyleBackColor = true;
            this.rb_switch_display_permanent.CheckedChanged += new System.EventHandler(this.rb_switch_display_permanent_CheckedChanged);
            // 
            // rb_switch_display_temp
            // 
            this.rb_switch_display_temp.AutoSize = true;
            this.rb_switch_display_temp.Checked = true;
            this.rb_switch_display_temp.ForeColor = System.Drawing.Color.White;
            this.rb_switch_display_temp.Location = new System.Drawing.Point(98, 48);
            this.rb_switch_display_temp.Name = "rb_switch_display_temp";
            this.rb_switch_display_temp.Size = new System.Drawing.Size(569, 24);
            this.rb_switch_display_temp.TabIndex = 11;
            this.rb_switch_display_temp.TabStop = true;
            this.rb_switch_display_temp.Text = "Revert back to original Display Profile (temporary change while running game)";
            this.rb_switch_display_temp.UseVisualStyleBackColor = true;
            this.rb_switch_display_temp.CheckedChanged += new System.EventHandler(this.rb_switch_display_temp_CheckedChanged);
            // 
            // txt_shortcut_save_name
            // 
            this.txt_shortcut_save_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_shortcut_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_shortcut_save_name.Location = new System.Drawing.Point(207, 731);
            this.txt_shortcut_save_name.MaxLength = 200;
            this.txt_shortcut_save_name.Name = "txt_shortcut_save_name";
            this.txt_shortcut_save_name.Size = new System.Drawing.Size(744, 35);
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
            this.lbl_shortcut_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_shortcut_name.AutoSize = true;
            this.lbl_shortcut_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_shortcut_name.ForeColor = System.Drawing.Color.Transparent;
            this.lbl_shortcut_name.Location = new System.Drawing.Point(23, 734);
            this.lbl_shortcut_name.Name = "lbl_shortcut_name";
            this.lbl_shortcut_name.Size = new System.Drawing.Size(178, 29);
            this.lbl_shortcut_name.TabIndex = 31;
            this.lbl_shortcut_name.Text = "Shortcut Name:";
            // 
            // cb_autosuggest
            // 
            this.cb_autosuggest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_autosuggest.AutoSize = true;
            this.cb_autosuggest.Checked = true;
            this.cb_autosuggest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_autosuggest.ForeColor = System.Drawing.Color.White;
            this.cb_autosuggest.Location = new System.Drawing.Point(969, 740);
            this.cb_autosuggest.Name = "cb_autosuggest";
            this.cb_autosuggest.Size = new System.Drawing.Size(117, 17);
            this.cb_autosuggest.TabIndex = 32;
            this.cb_autosuggest.Text = "Auto-suggest name";
            this.cb_autosuggest.UseVisualStyleBackColor = true;
            this.cb_autosuggest.CheckedChanged += new System.EventHandler(this.cb_autosuggest_CheckedChanged);
            // 
            // btn_hotkey
            // 
            this.btn_hotkey.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_hotkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_hotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_hotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_hotkey.ForeColor = System.Drawing.Color.White;
            this.btn_hotkey.Location = new System.Drawing.Point(434, 778);
            this.btn_hotkey.Name = "btn_hotkey";
            this.btn_hotkey.Size = new System.Drawing.Size(120, 40);
            this.btn_hotkey.TabIndex = 36;
            this.btn_hotkey.Text = "&Hotkey";
            this.btn_hotkey.UseVisualStyleBackColor = true;
            this.btn_hotkey.Click += new System.EventHandler(this.btn_hotkey_Click);
            // 
            // lbl_hotkey_assigned
            // 
            this.lbl_hotkey_assigned.AutoSize = true;
            this.lbl_hotkey_assigned.BackColor = System.Drawing.Color.Transparent;
            this.lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey_assigned.Location = new System.Drawing.Point(26, 793);
            this.lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            this.lbl_hotkey_assigned.Size = new System.Drawing.Size(57, 16);
            this.lbl_hotkey_assigned.TabIndex = 37;
            this.lbl_hotkey_assigned.Text = "Hotkey: ";
            this.lbl_hotkey_assigned.Visible = false;
            this.lbl_hotkey_assigned.Click += new System.EventHandler(this.lbl_hotkey_assigned_Click);
            // 
            // btn_find_examples
            // 
            this.btn_find_examples.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_find_examples.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_find_examples.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_find_examples.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_find_examples.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_find_examples.ForeColor = System.Drawing.Color.White;
            this.btn_find_examples.Location = new System.Drawing.Point(976, 72);
            this.btn_find_examples.Name = "btn_find_examples";
            this.btn_find_examples.Size = new System.Drawing.Size(94, 25);
            this.btn_find_examples.TabIndex = 40;
            this.btn_find_examples.Text = "Find &Examples";
            this.btn_find_examples.UseVisualStyleBackColor = true;
            this.btn_find_examples.Click += new System.EventHandler(this.btn_find_examples_Click);
            // 
            // ShortcutForm
            // 
            this.AcceptButton = this.btn_save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(1114, 830);
            this.Controls.Add(this.lbl_hotkey_assigned);
            this.Controls.Add(this.btn_hotkey);
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
            this.gb_capture_settings.ResumeLayout(false);
            this.gb_capture_settings.PerformLayout();
            this.gb_capture_volume.ResumeLayout(false);
            this.gb_capture_volume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_capture_volume)).EndInit();
            this.gb_audio_settings.ResumeLayout(false);
            this.gb_audio_settings.PerformLayout();
            this.gb_audio_volume.ResumeLayout(false);
            this.gb_audio_volume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_audio_volume)).EndInit();
            this.tabp_before.ResumeLayout(false);
            this.tabp_before.PerformLayout();
            this.tabp_game.ResumeLayout(false);
            this.tabp_game.PerformLayout();
            this.p_standalone.ResumeLayout(false);
            this.p_standalone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_executable)).EndInit();
            this.p_game.ResumeLayout(false);
            this.p_game.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout_game)).EndInit();
            this.tabp_after.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.TextBox txt_args_game;
        private System.Windows.Forms.CheckBox cb_args_game;
        private System.Windows.Forms.Label lbl_game_timeout;
        private System.Windows.Forms.NumericUpDown nud_timeout_game;
        private System.Windows.Forms.RadioButton rb_launcher;
        private System.Windows.Forms.RadioButton rb_no_game;
        private System.Windows.Forms.Panel p_standalone;
        private System.Windows.Forms.TextBox txt_args_executable;
        private System.Windows.Forms.CheckBox cb_args_executable;
        private System.Windows.Forms.Button btn_choose_alternative_executable;
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
        private System.Windows.Forms.TabPage tabp_audio;
        private System.Windows.Forms.GroupBox gb_display_after;
        private System.Windows.Forms.RadioButton rb_switch_display_permanent;
        private System.Windows.Forms.RadioButton rb_switch_display_temp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_switch_audio_permanent;
        private System.Windows.Forms.RadioButton rb_switch_audio_temp;
        private System.Windows.Forms.GroupBox gb_capture_settings;
        private System.Windows.Forms.GroupBox gb_capture_volume;
        private System.Windows.Forms.RadioButton rb_set_capture_volume;
        private System.Windows.Forms.RadioButton rb_keep_capture_volume;
        private System.Windows.Forms.Label lbl_capture_volume;
        private System.Windows.Forms.NumericUpDown nud_capture_volume;
        private System.Windows.Forms.Button btn_rescan_capture;
        private System.Windows.Forms.ComboBox cb_capture_device;
        private System.Windows.Forms.RadioButton rb_change_capture;
        private System.Windows.Forms.RadioButton rb_no_change_capture;
        private System.Windows.Forms.GroupBox gb_audio_settings;
        private System.Windows.Forms.GroupBox gb_audio_volume;
        private System.Windows.Forms.RadioButton rb_set_audio_volume;
        private System.Windows.Forms.RadioButton rb_keep_audio_volume;
        private System.Windows.Forms.Label lbl_audio_volume;
        private System.Windows.Forms.NumericUpDown nud_audio_volume;
        private System.Windows.Forms.Button btn_rescan_audio;
        private System.Windows.Forms.ComboBox cb_audio_device;
        private System.Windows.Forms.RadioButton rb_change_audio;
        private System.Windows.Forms.RadioButton rb_no_change_audio;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rb_switch_capture_permanent;
        private System.Windows.Forms.RadioButton rb_switch_capture_temp;
        private System.Windows.Forms.Label lbl_no_game_libraries;
        private System.Windows.Forms.Label lbl_disabled_shortcut_audio_chipset;
        private System.Windows.Forms.Label lbl_no_active_audio_devices;
        private System.Windows.Forms.Label lbl_no_active_capture_devices;
        private System.Windows.Forms.Button btn_choose_alternative_game;
        private System.Windows.Forms.TextBox txt_alternative_game;
        private System.Windows.Forms.CheckBox cb_wait_alternative_game;
        private System.Windows.Forms.Button btn_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_assigned;
        private System.Windows.Forms.FlowLayoutPanel flp_start_programs;
        private System.Windows.Forms.Button btn_add_new_start_program;
        private System.Windows.Forms.Label label3;
        internal Manina.Windows.Forms.ImageListView ilv_games;
        private System.Windows.Forms.Button btn_find_examples;
    }
}