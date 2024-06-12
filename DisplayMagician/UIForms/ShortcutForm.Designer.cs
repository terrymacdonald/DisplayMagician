using DisplayMagician.Resources;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutForm));
            btn_save = new System.Windows.Forms.Button();
            btn_cancel = new System.Windows.Forms.Button();
            il_games = new System.Windows.Forms.ImageList(components);
            dialog_open = new System.Windows.Forms.OpenFileDialog();
            tabc_shortcut = new System.Windows.Forms.TabControl();
            tabp_display = new System.Windows.Forms.TabPage();
            ilv_saved_profiles = new Manina.Windows.Forms.ImageListView();
            p_profiles = new System.Windows.Forms.Panel();
            dv_profile = new DisplayView();
            pbLogo = new System.Windows.Forms.PictureBox();
            lbl_profile_shown_subtitle = new System.Windows.Forms.Label();
            lbl_profile_shown = new System.Windows.Forms.Label();
            tabp_audio = new System.Windows.Forms.TabPage();
            lbl_no_active_capture_devices = new System.Windows.Forms.Label();
            lbl_no_active_audio_devices = new System.Windows.Forms.Label();
            lbl_disabled_shortcut_audio_chipset = new System.Windows.Forms.Label();
            gb_capture_settings = new System.Windows.Forms.GroupBox();
            cb_capture_comms_device = new System.Windows.Forms.CheckBox();
            gb_capture_volume = new System.Windows.Forms.GroupBox();
            rb_set_capture_volume = new System.Windows.Forms.RadioButton();
            rb_keep_capture_volume = new System.Windows.Forms.RadioButton();
            lbl_capture_volume = new System.Windows.Forms.Label();
            nud_capture_volume = new System.Windows.Forms.NumericUpDown();
            btn_rescan_capture = new System.Windows.Forms.Button();
            cb_capture_device = new System.Windows.Forms.ComboBox();
            rb_change_capture = new System.Windows.Forms.RadioButton();
            rb_no_change_capture = new System.Windows.Forms.RadioButton();
            gb_audio_settings = new System.Windows.Forms.GroupBox();
            cb_audio_comms_device = new System.Windows.Forms.CheckBox();
            gb_audio_volume = new System.Windows.Forms.GroupBox();
            rb_set_audio_volume = new System.Windows.Forms.RadioButton();
            rb_keep_audio_volume = new System.Windows.Forms.RadioButton();
            lbl_audio_volume = new System.Windows.Forms.Label();
            nud_audio_volume = new System.Windows.Forms.NumericUpDown();
            btn_rescan_audio = new System.Windows.Forms.Button();
            cb_audio_device = new System.Windows.Forms.ComboBox();
            rb_change_audio = new System.Windows.Forms.RadioButton();
            rb_no_change_audio = new System.Windows.Forms.RadioButton();
            tabp_before = new System.Windows.Forms.TabPage();
            flp_start_programs = new System.Windows.Forms.FlowLayoutPanel();
            p_start_program_upper = new System.Windows.Forms.Panel();
            btn_find_examples_startprograms = new System.Windows.Forms.Button();
            btn_add_new_start_program = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            tabp_game = new System.Windows.Forms.TabPage();
            p_game_list = new System.Windows.Forms.Panel();
            ilv_games = new Manina.Windows.Forms.ImageListView();
            p_gametostart = new System.Windows.Forms.Panel();
            btn_find_examples_game = new System.Windows.Forms.Button();
            p_standalone = new System.Windows.Forms.Panel();
            cb_run_exe_as_administrator = new System.Windows.Forms.CheckBox();
            btn_choose_exe_icon = new System.Windows.Forms.Button();
            pb_exe_icon = new System.Windows.Forms.PictureBox();
            cbx_exe_priority = new System.Windows.Forms.ComboBox();
            lbl_exe_priority = new System.Windows.Forms.Label();
            btn_exe_to_start = new System.Windows.Forms.Button();
            txt_args_executable = new System.Windows.Forms.TextBox();
            cb_args_executable = new System.Windows.Forms.CheckBox();
            btn_choose_alternative_executable = new System.Windows.Forms.Button();
            txt_alternative_executable = new System.Windows.Forms.TextBox();
            rb_wait_alternative_executable = new System.Windows.Forms.RadioButton();
            rb_wait_executable = new System.Windows.Forms.RadioButton();
            txt_executable = new System.Windows.Forms.TextBox();
            lbl_app_executable = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            nud_timeout_executable = new System.Windows.Forms.NumericUpDown();
            rb_standalone = new System.Windows.Forms.RadioButton();
            rb_no_game = new System.Windows.Forms.RadioButton();
            p_game = new System.Windows.Forms.Panel();
            btn_refresh_games_list = new System.Windows.Forms.Button();
            btn_choose_game_icon = new System.Windows.Forms.Button();
            pb_game_icon = new System.Windows.Forms.PictureBox();
            lbl_no_game_libraries = new System.Windows.Forms.Label();
            cbx_game_priority = new System.Windows.Forms.ComboBox();
            cb_wait_alternative_game = new System.Windows.Forms.CheckBox();
            btn_choose_alternative_game = new System.Windows.Forms.Button();
            txt_alternative_game = new System.Windows.Forms.TextBox();
            txt_game_name = new System.Windows.Forms.TextBox();
            lbl_game_priority = new System.Windows.Forms.Label();
            lbl_game_name = new System.Windows.Forms.Label();
            txt_args_game = new System.Windows.Forms.TextBox();
            cb_args_game = new System.Windows.Forms.CheckBox();
            lbl_game_timeout = new System.Windows.Forms.Label();
            nud_timeout_game = new System.Windows.Forms.NumericUpDown();
            lbl_game_library = new System.Windows.Forms.Label();
            rb_launcher = new System.Windows.Forms.RadioButton();
            tabp_after = new System.Windows.Forms.TabPage();
            groupBox3 = new System.Windows.Forms.GroupBox();
            cb_run_cmd_afterwards_run_as_administrator = new System.Windows.Forms.CheckBox();
            cb_run_cmd_afterwards_dont_start = new System.Windows.Forms.CheckBox();
            txt_run_cmd_afterwards_args = new System.Windows.Forms.TextBox();
            cb_run_cmd_afterwards_args = new System.Windows.Forms.CheckBox();
            btn_run_cmd_afterwards = new System.Windows.Forms.Button();
            txt_run_cmd_afterwards = new System.Windows.Forms.TextBox();
            cb_run_cmd_afterwards = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            rb_switch_capture_permanent = new System.Windows.Forms.RadioButton();
            rb_switch_capture_temp = new System.Windows.Forms.RadioButton();
            groupBox1 = new System.Windows.Forms.GroupBox();
            rb_switch_audio_permanent = new System.Windows.Forms.RadioButton();
            rb_switch_audio_temp = new System.Windows.Forms.RadioButton();
            gb_display_after = new System.Windows.Forms.GroupBox();
            rb_switch_display_permanent = new System.Windows.Forms.RadioButton();
            rb_switch_display_temp = new System.Windows.Forms.RadioButton();
            txt_shortcut_save_name = new System.Windows.Forms.TextBox();
            lbl_title = new System.Windows.Forms.Label();
            lbl_shortcut_name = new System.Windows.Forms.Label();
            cb_autosuggest = new System.Windows.Forms.CheckBox();
            btn_hotkey = new System.Windows.Forms.Button();
            lbl_hotkey_assigned = new System.Windows.Forms.Label();
            btn_help = new System.Windows.Forms.Button();
            tabc_shortcut.SuspendLayout();
            tabp_display.SuspendLayout();
            p_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
            tabp_audio.SuspendLayout();
            gb_capture_settings.SuspendLayout();
            gb_capture_volume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_capture_volume).BeginInit();
            gb_audio_settings.SuspendLayout();
            gb_audio_volume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_audio_volume).BeginInit();
            tabp_before.SuspendLayout();
            p_start_program_upper.SuspendLayout();
            tabp_game.SuspendLayout();
            p_game_list.SuspendLayout();
            p_gametostart.SuspendLayout();
            p_standalone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_exe_icon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_timeout_executable).BeginInit();
            p_game.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_game_icon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_timeout_game).BeginInit();
            tabp_after.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            gb_display_after.SuspendLayout();
            SuspendLayout();
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(636, 1028);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(140, 35);
            btn_save.TabIndex = 6;
            btn_save.Text = "&Save";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += btn_save_Click;
            // 
            // btn_cancel
            // 
            btn_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_cancel.ForeColor = System.Drawing.Color.White;
            btn_cancel.Location = new System.Drawing.Point(1141, 1045);
            btn_cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(110, 29);
            btn_cancel.TabIndex = 5;
            btn_cancel.Text = "&Back";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += btn_back_Click;
            // 
            // il_games
            // 
            il_games.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            il_games.ImageSize = new System.Drawing.Size(32, 32);
            il_games.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // dialog_open
            // 
            dialog_open.DefaultExt = "exe";
            dialog_open.Filter = "Executables (*.exe; *.com; *.bat; *.cmd; *.ps1)|*.exe; *.com; *.bat; *.cmd; *.ps1|All files (*.*)|*.*";
            dialog_open.RestoreDirectory = true;
            dialog_open.SupportMultiDottedExtensions = true;
            // 
            // tabc_shortcut
            // 
            tabc_shortcut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabc_shortcut.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            tabc_shortcut.Controls.Add(tabp_display);
            tabc_shortcut.Controls.Add(tabp_audio);
            tabc_shortcut.Controls.Add(tabp_before);
            tabc_shortcut.Controls.Add(tabp_game);
            tabc_shortcut.Controls.Add(tabp_after);
            tabc_shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabc_shortcut.HotTrack = true;
            tabc_shortcut.Location = new System.Drawing.Point(14, 72);
            tabc_shortcut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabc_shortcut.Name = "tabc_shortcut";
            tabc_shortcut.SelectedIndex = 0;
            tabc_shortcut.ShowToolTips = true;
            tabc_shortcut.Size = new System.Drawing.Size(1237, 885);
            tabc_shortcut.TabIndex = 28;
            // 
            // tabp_display
            // 
            tabp_display.BackColor = System.Drawing.Color.Black;
            tabp_display.Controls.Add(ilv_saved_profiles);
            tabp_display.Controls.Add(p_profiles);
            tabp_display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabp_display.ForeColor = System.Drawing.Color.White;
            tabp_display.Location = new System.Drawing.Point(4, 32);
            tabp_display.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_display.Name = "tabp_display";
            tabp_display.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_display.Size = new System.Drawing.Size(1229, 849);
            tabp_display.TabIndex = 0;
            tabp_display.Text = "1. Choose Display Profile";
            tabp_display.ToolTipText = "Choose which previously saved Display Profile you will use with this shortcut.";
            // 
            // ilv_saved_profiles
            // 
            ilv_saved_profiles.AllowCheckBoxClick = false;
            ilv_saved_profiles.AllowColumnClick = false;
            ilv_saved_profiles.AllowColumnResize = false;
            ilv_saved_profiles.AllowItemReorder = false;
            ilv_saved_profiles.AllowPaneResize = false;
            ilv_saved_profiles.Dock = System.Windows.Forms.DockStyle.Fill;
            ilv_saved_profiles.Location = new System.Drawing.Point(4, 551);
            ilv_saved_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.Name = "ilv_saved_profiles";
            ilv_saved_profiles.PersistentCacheDirectory = "";
            ilv_saved_profiles.PersistentCacheSize = 100L;
            ilv_saved_profiles.Size = new System.Drawing.Size(1221, 295);
            ilv_saved_profiles.TabIndex = 24;
            ilv_saved_profiles.UseWIC = true;
            ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            ilv_saved_profiles.ItemClick += ilv_saved_profiles_ItemClick;
            // 
            // p_profiles
            // 
            p_profiles.Controls.Add(dv_profile);
            p_profiles.Controls.Add(pbLogo);
            p_profiles.Controls.Add(lbl_profile_shown_subtitle);
            p_profiles.Controls.Add(lbl_profile_shown);
            p_profiles.Dock = System.Windows.Forms.DockStyle.Top;
            p_profiles.Location = new System.Drawing.Point(4, 3);
            p_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_profiles.Name = "p_profiles";
            p_profiles.Size = new System.Drawing.Size(1221, 548);
            p_profiles.TabIndex = 39;
            // 
            // dv_profile
            // 
            dv_profile.BackColor = System.Drawing.Color.DimGray;
            dv_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dv_profile.Dock = System.Windows.Forms.DockStyle.Top;
            dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            dv_profile.Location = new System.Drawing.Point(0, 0);
            dv_profile.Margin = new System.Windows.Forms.Padding(21);
            dv_profile.Name = "dv_profile";
            dv_profile.PaddingX = 100;
            dv_profile.PaddingY = 100;
            dv_profile.Profile = null;
            dv_profile.Size = new System.Drawing.Size(1221, 548);
            dv_profile.TabIndex = 23;
            // 
            // pbLogo
            // 
            pbLogo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            pbLogo.BackColor = System.Drawing.Color.DimGray;
            pbLogo.Location = new System.Drawing.Point(1049, 30);
            pbLogo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new System.Drawing.Size(117, 57);
            pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 38;
            pbLogo.TabStop = false;
            // 
            // lbl_profile_shown_subtitle
            // 
            lbl_profile_shown_subtitle.AutoSize = true;
            lbl_profile_shown_subtitle.BackColor = System.Drawing.Color.DimGray;
            lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown_subtitle.Location = new System.Drawing.Point(4, 63);
            lbl_profile_shown_subtitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            lbl_profile_shown_subtitle.Size = new System.Drawing.Size(397, 20);
            lbl_profile_shown_subtitle.TabIndex = 26;
            lbl_profile_shown_subtitle.Text = "Please select a Display Profile to use with this Shortcut.";
            // 
            // lbl_profile_shown
            // 
            lbl_profile_shown.AutoSize = true;
            lbl_profile_shown.BackColor = System.Drawing.Color.DimGray;
            lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown.Location = new System.Drawing.Point(4, 30);
            lbl_profile_shown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_profile_shown.Name = "lbl_profile_shown";
            lbl_profile_shown.Size = new System.Drawing.Size(308, 29);
            lbl_profile_shown.TabIndex = 25;
            lbl_profile_shown.Text = "No Display Profile Selected";
            // 
            // tabp_audio
            // 
            tabp_audio.BackColor = System.Drawing.Color.Black;
            tabp_audio.Controls.Add(lbl_no_active_capture_devices);
            tabp_audio.Controls.Add(lbl_no_active_audio_devices);
            tabp_audio.Controls.Add(lbl_disabled_shortcut_audio_chipset);
            tabp_audio.Controls.Add(gb_capture_settings);
            tabp_audio.Controls.Add(gb_audio_settings);
            tabp_audio.Location = new System.Drawing.Point(4, 32);
            tabp_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_audio.Name = "tabp_audio";
            tabp_audio.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_audio.Size = new System.Drawing.Size(1229, 849);
            tabp_audio.TabIndex = 4;
            tabp_audio.Text = "2. Choose Audio";
            // 
            // lbl_no_active_capture_devices
            // 
            lbl_no_active_capture_devices.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_no_active_capture_devices.AutoSize = true;
            lbl_no_active_capture_devices.BackColor = System.Drawing.Color.Brown;
            lbl_no_active_capture_devices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_no_active_capture_devices.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_no_active_capture_devices.ForeColor = System.Drawing.Color.White;
            lbl_no_active_capture_devices.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_no_active_capture_devices.Location = new System.Drawing.Point(130, 505);
            lbl_no_active_capture_devices.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_no_active_capture_devices.Name = "lbl_no_active_capture_devices";
            lbl_no_active_capture_devices.Size = new System.Drawing.Size(831, 22);
            lbl_no_active_capture_devices.TabIndex = 36;
            lbl_no_active_capture_devices.Text = "No active microphone inputs found. Please connect or enable at least one microphone if you want to use this feature.";
            lbl_no_active_capture_devices.Visible = false;
            // 
            // lbl_no_active_audio_devices
            // 
            lbl_no_active_audio_devices.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_no_active_audio_devices.AutoSize = true;
            lbl_no_active_audio_devices.BackColor = System.Drawing.Color.Brown;
            lbl_no_active_audio_devices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_no_active_audio_devices.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_no_active_audio_devices.ForeColor = System.Drawing.Color.White;
            lbl_no_active_audio_devices.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_no_active_audio_devices.Location = new System.Drawing.Point(135, 177);
            lbl_no_active_audio_devices.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_no_active_audio_devices.Name = "lbl_no_active_audio_devices";
            lbl_no_active_audio_devices.Size = new System.Drawing.Size(804, 22);
            lbl_no_active_audio_devices.TabIndex = 35;
            lbl_no_active_audio_devices.Text = "No active audio outputs found. Please connect or enable at least one audio output if you want to use this feature.";
            lbl_no_active_audio_devices.Visible = false;
            // 
            // lbl_disabled_shortcut_audio_chipset
            // 
            lbl_disabled_shortcut_audio_chipset.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_disabled_shortcut_audio_chipset.AutoSize = true;
            lbl_disabled_shortcut_audio_chipset.BackColor = System.Drawing.Color.Brown;
            lbl_disabled_shortcut_audio_chipset.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_disabled_shortcut_audio_chipset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_disabled_shortcut_audio_chipset.ForeColor = System.Drawing.Color.White;
            lbl_disabled_shortcut_audio_chipset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_disabled_shortcut_audio_chipset.Location = new System.Drawing.Point(289, 350);
            lbl_disabled_shortcut_audio_chipset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_disabled_shortcut_audio_chipset.Name = "lbl_disabled_shortcut_audio_chipset";
            lbl_disabled_shortcut_audio_chipset.Size = new System.Drawing.Size(557, 22);
            lbl_disabled_shortcut_audio_chipset.TabIndex = 34;
            lbl_disabled_shortcut_audio_chipset.Text = "Unsupported Audio Chipset. Setting audio isn't supported on your computer :(";
            lbl_disabled_shortcut_audio_chipset.Visible = false;
            // 
            // gb_capture_settings
            // 
            gb_capture_settings.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_capture_settings.Controls.Add(cb_capture_comms_device);
            gb_capture_settings.Controls.Add(gb_capture_volume);
            gb_capture_settings.Controls.Add(btn_rescan_capture);
            gb_capture_settings.Controls.Add(cb_capture_device);
            gb_capture_settings.Controls.Add(rb_change_capture);
            gb_capture_settings.Controls.Add(rb_no_change_capture);
            gb_capture_settings.ForeColor = System.Drawing.Color.White;
            gb_capture_settings.Location = new System.Drawing.Point(56, 437);
            gb_capture_settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_capture_settings.Name = "gb_capture_settings";
            gb_capture_settings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_capture_settings.Size = new System.Drawing.Size(1112, 353);
            gb_capture_settings.TabIndex = 21;
            gb_capture_settings.TabStop = false;
            gb_capture_settings.Text = "Microphone Settings";
            // 
            // cb_capture_comms_device
            // 
            cb_capture_comms_device.AutoSize = true;
            cb_capture_comms_device.Location = new System.Drawing.Point(379, 132);
            cb_capture_comms_device.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_capture_comms_device.Name = "cb_capture_comms_device";
            cb_capture_comms_device.Size = new System.Drawing.Size(418, 24);
            cb_capture_comms_device.TabIndex = 22;
            cb_capture_comms_device.Text = "Also set this as the default communicatons microphone";
            cb_capture_comms_device.UseVisualStyleBackColor = true;
            // 
            // gb_capture_volume
            // 
            gb_capture_volume.Controls.Add(rb_set_capture_volume);
            gb_capture_volume.Controls.Add(rb_keep_capture_volume);
            gb_capture_volume.Controls.Add(lbl_capture_volume);
            gb_capture_volume.Controls.Add(nud_capture_volume);
            gb_capture_volume.ForeColor = System.Drawing.Color.White;
            gb_capture_volume.Location = new System.Drawing.Point(382, 166);
            gb_capture_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_capture_volume.Name = "gb_capture_volume";
            gb_capture_volume.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_capture_volume.Size = new System.Drawing.Size(500, 148);
            gb_capture_volume.TabIndex = 20;
            gb_capture_volume.TabStop = false;
            gb_capture_volume.Text = "Microphone Volume";
            gb_capture_volume.Visible = false;
            // 
            // rb_set_capture_volume
            // 
            rb_set_capture_volume.AutoSize = true;
            rb_set_capture_volume.ForeColor = System.Drawing.Color.White;
            rb_set_capture_volume.Location = new System.Drawing.Point(72, 90);
            rb_set_capture_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_set_capture_volume.Name = "rb_set_capture_volume";
            rb_set_capture_volume.Size = new System.Drawing.Size(167, 24);
            rb_set_capture_volume.TabIndex = 13;
            rb_set_capture_volume.Text = "Set audio volume at";
            rb_set_capture_volume.UseVisualStyleBackColor = true;
            rb_set_capture_volume.CheckedChanged += rb_set_capture_volume_CheckedChanged;
            // 
            // rb_keep_capture_volume
            // 
            rb_keep_capture_volume.AutoSize = true;
            rb_keep_capture_volume.Checked = true;
            rb_keep_capture_volume.ForeColor = System.Drawing.Color.White;
            rb_keep_capture_volume.Location = new System.Drawing.Point(72, 40);
            rb_keep_capture_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_keep_capture_volume.Name = "rb_keep_capture_volume";
            rb_keep_capture_volume.Size = new System.Drawing.Size(203, 24);
            rb_keep_capture_volume.TabIndex = 12;
            rb_keep_capture_volume.TabStop = true;
            rb_keep_capture_volume.Text = "Leave audio volume as is";
            rb_keep_capture_volume.UseVisualStyleBackColor = true;
            rb_keep_capture_volume.CheckedChanged += rb_keep_capture_volume_CheckedChanged;
            // 
            // lbl_capture_volume
            // 
            lbl_capture_volume.AutoSize = true;
            lbl_capture_volume.ForeColor = System.Drawing.Color.White;
            lbl_capture_volume.Location = new System.Drawing.Point(349, 92);
            lbl_capture_volume.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_capture_volume.Name = "lbl_capture_volume";
            lbl_capture_volume.Size = new System.Drawing.Size(63, 20);
            lbl_capture_volume.TabIndex = 11;
            lbl_capture_volume.Text = "percent";
            // 
            // nud_capture_volume
            // 
            nud_capture_volume.Enabled = false;
            nud_capture_volume.Location = new System.Drawing.Point(272, 90);
            nud_capture_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_capture_volume.Name = "nud_capture_volume";
            nud_capture_volume.Size = new System.Drawing.Size(70, 26);
            nud_capture_volume.TabIndex = 10;
            nud_capture_volume.Value = new decimal(new int[] { 100, 0, 0, 0 });
            nud_capture_volume.ValueChanged += nud_capture_volume_ValueChanged;
            // 
            // btn_rescan_capture
            // 
            btn_rescan_capture.Enabled = false;
            btn_rescan_capture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rescan_capture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rescan_capture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rescan_capture.ForeColor = System.Drawing.Color.White;
            btn_rescan_capture.Location = new System.Drawing.Point(887, 84);
            btn_rescan_capture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_rescan_capture.Name = "btn_rescan_capture";
            btn_rescan_capture.Size = new System.Drawing.Size(83, 32);
            btn_rescan_capture.TabIndex = 19;
            btn_rescan_capture.Text = "rescan";
            btn_rescan_capture.UseVisualStyleBackColor = true;
            btn_rescan_capture.Click += btn_rescan_capture_Click;
            // 
            // cb_capture_device
            // 
            cb_capture_device.Enabled = false;
            cb_capture_device.FormattingEnabled = true;
            cb_capture_device.Location = new System.Drawing.Point(379, 84);
            cb_capture_device.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_capture_device.Name = "cb_capture_device";
            cb_capture_device.Size = new System.Drawing.Size(500, 28);
            cb_capture_device.TabIndex = 18;
            cb_capture_device.SelectedIndexChanged += cb_capture_device_SelectedIndexChanged;
            // 
            // rb_change_capture
            // 
            rb_change_capture.AutoSize = true;
            rb_change_capture.ForeColor = System.Drawing.Color.White;
            rb_change_capture.Location = new System.Drawing.Point(141, 84);
            rb_change_capture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_change_capture.Name = "rb_change_capture";
            rb_change_capture.Size = new System.Drawing.Size(192, 24);
            rb_change_capture.TabIndex = 17;
            rb_change_capture.Text = "Change microphone to:";
            rb_change_capture.UseVisualStyleBackColor = true;
            rb_change_capture.CheckedChanged += rb_change_capture_CheckedChanged;
            // 
            // rb_no_change_capture
            // 
            rb_no_change_capture.AutoSize = true;
            rb_no_change_capture.Checked = true;
            rb_no_change_capture.ForeColor = System.Drawing.Color.White;
            rb_no_change_capture.Location = new System.Drawing.Point(141, 40);
            rb_no_change_capture.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_no_change_capture.Name = "rb_no_change_capture";
            rb_no_change_capture.Size = new System.Drawing.Size(308, 24);
            rb_no_change_capture.TabIndex = 16;
            rb_no_change_capture.TabStop = true;
            rb_no_change_capture.Text = "Don't change microphone input settings";
            rb_no_change_capture.UseVisualStyleBackColor = true;
            rb_no_change_capture.CheckedChanged += rb_no_change_capture_CheckedChanged;
            // 
            // gb_audio_settings
            // 
            gb_audio_settings.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_audio_settings.Controls.Add(cb_audio_comms_device);
            gb_audio_settings.Controls.Add(gb_audio_volume);
            gb_audio_settings.Controls.Add(btn_rescan_audio);
            gb_audio_settings.Controls.Add(cb_audio_device);
            gb_audio_settings.Controls.Add(rb_change_audio);
            gb_audio_settings.Controls.Add(rb_no_change_audio);
            gb_audio_settings.ForeColor = System.Drawing.Color.White;
            gb_audio_settings.Location = new System.Drawing.Point(56, 35);
            gb_audio_settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_audio_settings.Name = "gb_audio_settings";
            gb_audio_settings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_audio_settings.Size = new System.Drawing.Size(1112, 369);
            gb_audio_settings.TabIndex = 0;
            gb_audio_settings.TabStop = false;
            gb_audio_settings.Text = "Audio Output Settings";
            // 
            // cb_audio_comms_device
            // 
            cb_audio_comms_device.AutoSize = true;
            cb_audio_comms_device.Location = new System.Drawing.Point(380, 129);
            cb_audio_comms_device.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_audio_comms_device.Name = "cb_audio_comms_device";
            cb_audio_comms_device.Size = new System.Drawing.Size(424, 24);
            cb_audio_comms_device.TabIndex = 21;
            cb_audio_comms_device.Text = "Also set this as the default communicatons audio output";
            cb_audio_comms_device.UseVisualStyleBackColor = true;
            // 
            // gb_audio_volume
            // 
            gb_audio_volume.Controls.Add(rb_set_audio_volume);
            gb_audio_volume.Controls.Add(rb_keep_audio_volume);
            gb_audio_volume.Controls.Add(lbl_audio_volume);
            gb_audio_volume.Controls.Add(nud_audio_volume);
            gb_audio_volume.ForeColor = System.Drawing.Color.White;
            gb_audio_volume.Location = new System.Drawing.Point(379, 167);
            gb_audio_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_audio_volume.Name = "gb_audio_volume";
            gb_audio_volume.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_audio_volume.Size = new System.Drawing.Size(500, 153);
            gb_audio_volume.TabIndex = 20;
            gb_audio_volume.TabStop = false;
            gb_audio_volume.Text = "Audio Output Volume";
            gb_audio_volume.Visible = false;
            // 
            // rb_set_audio_volume
            // 
            rb_set_audio_volume.AutoSize = true;
            rb_set_audio_volume.ForeColor = System.Drawing.Color.White;
            rb_set_audio_volume.Location = new System.Drawing.Point(71, 95);
            rb_set_audio_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_set_audio_volume.Name = "rb_set_audio_volume";
            rb_set_audio_volume.Size = new System.Drawing.Size(167, 24);
            rb_set_audio_volume.TabIndex = 13;
            rb_set_audio_volume.Text = "Set audio volume at";
            rb_set_audio_volume.UseVisualStyleBackColor = true;
            rb_set_audio_volume.CheckedChanged += rb_set_audio_volume_CheckedChanged;
            // 
            // rb_keep_audio_volume
            // 
            rb_keep_audio_volume.AutoSize = true;
            rb_keep_audio_volume.Checked = true;
            rb_keep_audio_volume.ForeColor = System.Drawing.Color.White;
            rb_keep_audio_volume.Location = new System.Drawing.Point(71, 44);
            rb_keep_audio_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_keep_audio_volume.Name = "rb_keep_audio_volume";
            rb_keep_audio_volume.Size = new System.Drawing.Size(203, 24);
            rb_keep_audio_volume.TabIndex = 12;
            rb_keep_audio_volume.TabStop = true;
            rb_keep_audio_volume.Text = "Leave audio volume as is";
            rb_keep_audio_volume.UseVisualStyleBackColor = true;
            rb_keep_audio_volume.CheckedChanged += rb_keep_audio_volume_CheckedChanged;
            // 
            // lbl_audio_volume
            // 
            lbl_audio_volume.AutoSize = true;
            lbl_audio_volume.ForeColor = System.Drawing.Color.White;
            lbl_audio_volume.Location = new System.Drawing.Point(348, 97);
            lbl_audio_volume.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_audio_volume.Name = "lbl_audio_volume";
            lbl_audio_volume.Size = new System.Drawing.Size(63, 20);
            lbl_audio_volume.TabIndex = 11;
            lbl_audio_volume.Text = "percent";
            // 
            // nud_audio_volume
            // 
            nud_audio_volume.Enabled = false;
            nud_audio_volume.Location = new System.Drawing.Point(271, 95);
            nud_audio_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_audio_volume.Name = "nud_audio_volume";
            nud_audio_volume.Size = new System.Drawing.Size(70, 26);
            nud_audio_volume.TabIndex = 10;
            nud_audio_volume.Value = new decimal(new int[] { 100, 0, 0, 0 });
            nud_audio_volume.ValueChanged += nud_audio_volume_ValueChanged;
            // 
            // btn_rescan_audio
            // 
            btn_rescan_audio.Enabled = false;
            btn_rescan_audio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_rescan_audio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_rescan_audio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_rescan_audio.ForeColor = System.Drawing.Color.White;
            btn_rescan_audio.Location = new System.Drawing.Point(887, 83);
            btn_rescan_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_rescan_audio.Name = "btn_rescan_audio";
            btn_rescan_audio.Size = new System.Drawing.Size(83, 32);
            btn_rescan_audio.TabIndex = 19;
            btn_rescan_audio.Text = "rescan";
            btn_rescan_audio.UseVisualStyleBackColor = true;
            btn_rescan_audio.Click += btn_rescan_audio_Click;
            // 
            // cb_audio_device
            // 
            cb_audio_device.Enabled = false;
            cb_audio_device.FormattingEnabled = true;
            cb_audio_device.Location = new System.Drawing.Point(379, 83);
            cb_audio_device.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_audio_device.Name = "cb_audio_device";
            cb_audio_device.Size = new System.Drawing.Size(500, 28);
            cb_audio_device.TabIndex = 18;
            cb_audio_device.SelectedIndexChanged += cb_audio_device_SelectedIndexChanged;
            // 
            // rb_change_audio
            // 
            rb_change_audio.AutoSize = true;
            rb_change_audio.ForeColor = System.Drawing.Color.White;
            rb_change_audio.Location = new System.Drawing.Point(141, 83);
            rb_change_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_change_audio.Name = "rb_change_audio";
            rb_change_audio.Size = new System.Drawing.Size(198, 24);
            rb_change_audio.TabIndex = 17;
            rb_change_audio.Text = "Change audio output to:";
            rb_change_audio.UseVisualStyleBackColor = true;
            rb_change_audio.CheckedChanged += rb_change_audio_CheckedChanged;
            // 
            // rb_no_change_audio
            // 
            rb_no_change_audio.AutoSize = true;
            rb_no_change_audio.Checked = true;
            rb_no_change_audio.ForeColor = System.Drawing.Color.White;
            rb_no_change_audio.Location = new System.Drawing.Point(141, 39);
            rb_no_change_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_no_change_audio.Name = "rb_no_change_audio";
            rb_no_change_audio.Size = new System.Drawing.Size(275, 24);
            rb_no_change_audio.TabIndex = 16;
            rb_no_change_audio.TabStop = true;
            rb_no_change_audio.Text = "Don't change audio output settings";
            rb_no_change_audio.UseVisualStyleBackColor = true;
            rb_no_change_audio.CheckedChanged += rb_no_change_audio_CheckedChanged;
            // 
            // tabp_before
            // 
            tabp_before.BackColor = System.Drawing.Color.Black;
            tabp_before.Controls.Add(flp_start_programs);
            tabp_before.Controls.Add(p_start_program_upper);
            tabp_before.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabp_before.ForeColor = System.Drawing.Color.White;
            tabp_before.Location = new System.Drawing.Point(4, 32);
            tabp_before.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_before.Name = "tabp_before";
            tabp_before.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_before.Size = new System.Drawing.Size(1229, 849);
            tabp_before.TabIndex = 1;
            tabp_before.Text = "3. Choose what happens before";
            // 
            // flp_start_programs
            // 
            flp_start_programs.AllowDrop = true;
            flp_start_programs.AutoScroll = true;
            flp_start_programs.AutoScrollMargin = new System.Drawing.Size(5, 0);
            flp_start_programs.AutoScrollMinSize = new System.Drawing.Size(5, 0);
            flp_start_programs.BackColor = System.Drawing.Color.White;
            flp_start_programs.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_start_programs.Location = new System.Drawing.Point(4, 143);
            flp_start_programs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flp_start_programs.Name = "flp_start_programs";
            flp_start_programs.Size = new System.Drawing.Size(1221, 703);
            flp_start_programs.TabIndex = 0;
            // 
            // p_start_program_upper
            // 
            p_start_program_upper.Controls.Add(btn_find_examples_startprograms);
            p_start_program_upper.Controls.Add(btn_add_new_start_program);
            p_start_program_upper.Controls.Add(label3);
            p_start_program_upper.Dock = System.Windows.Forms.DockStyle.Top;
            p_start_program_upper.Location = new System.Drawing.Point(4, 3);
            p_start_program_upper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_start_program_upper.Name = "p_start_program_upper";
            p_start_program_upper.Size = new System.Drawing.Size(1221, 140);
            p_start_program_upper.TabIndex = 41;
            // 
            // btn_find_examples_startprograms
            // 
            btn_find_examples_startprograms.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btn_find_examples_startprograms.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_find_examples_startprograms.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_find_examples_startprograms.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_find_examples_startprograms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_find_examples_startprograms.ForeColor = System.Drawing.Color.White;
            btn_find_examples_startprograms.Location = new System.Drawing.Point(1065, 90);
            btn_find_examples_startprograms.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_find_examples_startprograms.Name = "btn_find_examples_startprograms";
            btn_find_examples_startprograms.Size = new System.Drawing.Size(136, 29);
            btn_find_examples_startprograms.TabIndex = 43;
            btn_find_examples_startprograms.Text = "Show me &Examples";
            btn_find_examples_startprograms.UseVisualStyleBackColor = true;
            btn_find_examples_startprograms.Click += btn_find_examples_startprograms_Click;
            // 
            // btn_add_new_start_program
            // 
            btn_add_new_start_program.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btn_add_new_start_program.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_add_new_start_program.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_add_new_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_add_new_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_add_new_start_program.ForeColor = System.Drawing.Color.White;
            btn_add_new_start_program.Location = new System.Drawing.Point(467, 73);
            btn_add_new_start_program.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_add_new_start_program.Name = "btn_add_new_start_program";
            btn_add_new_start_program.Size = new System.Drawing.Size(287, 46);
            btn_add_new_start_program.TabIndex = 41;
            btn_add_new_start_program.Text = "&Add Start Program";
            btn_add_new_start_program.UseVisualStyleBackColor = true;
            btn_add_new_start_program.Click += btn_add_new_start_program_Click;
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(159, 24);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(807, 20);
            label3.TabIndex = 42;
            label3.Text = "Add one or more additional programs to start before the main Game starts. They will start in the order listed below.";
            // 
            // tabp_game
            // 
            tabp_game.BackColor = System.Drawing.Color.Black;
            tabp_game.Controls.Add(p_game_list);
            tabp_game.Controls.Add(p_gametostart);
            tabp_game.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabp_game.ForeColor = System.Drawing.Color.White;
            tabp_game.Location = new System.Drawing.Point(4, 32);
            tabp_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_game.Name = "tabp_game";
            tabp_game.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_game.Size = new System.Drawing.Size(1229, 849);
            tabp_game.TabIndex = 2;
            tabp_game.Text = "4. Choose Game to start";
            // 
            // p_game_list
            // 
            p_game_list.Controls.Add(ilv_games);
            p_game_list.Dock = System.Windows.Forms.DockStyle.Fill;
            p_game_list.Location = new System.Drawing.Point(4, 603);
            p_game_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_game_list.Name = "p_game_list";
            p_game_list.Size = new System.Drawing.Size(1221, 243);
            p_game_list.TabIndex = 44;
            // 
            // ilv_games
            // 
            ilv_games.AllowCheckBoxClick = false;
            ilv_games.AllowColumnClick = false;
            ilv_games.AllowColumnResize = false;
            ilv_games.AllowItemReorder = false;
            ilv_games.AllowPaneResize = false;
            ilv_games.Dock = System.Windows.Forms.DockStyle.Fill;
            ilv_games.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ilv_games.IntegralScroll = true;
            ilv_games.Location = new System.Drawing.Point(0, 0);
            ilv_games.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_games.Name = "ilv_games";
            ilv_games.PersistentCacheDirectory = "";
            ilv_games.PersistentCacheSize = 100L;
            ilv_games.Size = new System.Drawing.Size(1221, 243);
            ilv_games.SortOrder = Manina.Windows.Forms.SortOrder.Ascending;
            ilv_games.TabIndex = 43;
            ilv_games.UseWIC = true;
            ilv_games.ItemClick += ilv_games_ItemClick;
            // 
            // p_gametostart
            // 
            p_gametostart.Controls.Add(btn_find_examples_game);
            p_gametostart.Controls.Add(p_standalone);
            p_gametostart.Controls.Add(rb_standalone);
            p_gametostart.Controls.Add(rb_no_game);
            p_gametostart.Controls.Add(p_game);
            p_gametostart.Controls.Add(rb_launcher);
            p_gametostart.Dock = System.Windows.Forms.DockStyle.Top;
            p_gametostart.Location = new System.Drawing.Point(4, 3);
            p_gametostart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_gametostart.Name = "p_gametostart";
            p_gametostart.Size = new System.Drawing.Size(1221, 600);
            p_gametostart.TabIndex = 43;
            // 
            // btn_find_examples_game
            // 
            btn_find_examples_game.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_find_examples_game.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_find_examples_game.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_find_examples_game.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_find_examples_game.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_find_examples_game.ForeColor = System.Drawing.Color.White;
            btn_find_examples_game.Location = new System.Drawing.Point(1079, 13);
            btn_find_examples_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_find_examples_game.Name = "btn_find_examples_game";
            btn_find_examples_game.Size = new System.Drawing.Size(136, 29);
            btn_find_examples_game.TabIndex = 47;
            btn_find_examples_game.Text = "Show me &Examples";
            btn_find_examples_game.UseVisualStyleBackColor = true;
            btn_find_examples_game.Click += btn_find_examples_game_Click;
            // 
            // p_standalone
            // 
            p_standalone.Anchor = System.Windows.Forms.AnchorStyles.None;
            p_standalone.Controls.Add(cb_run_exe_as_administrator);
            p_standalone.Controls.Add(btn_choose_exe_icon);
            p_standalone.Controls.Add(pb_exe_icon);
            p_standalone.Controls.Add(cbx_exe_priority);
            p_standalone.Controls.Add(lbl_exe_priority);
            p_standalone.Controls.Add(btn_exe_to_start);
            p_standalone.Controls.Add(txt_args_executable);
            p_standalone.Controls.Add(cb_args_executable);
            p_standalone.Controls.Add(btn_choose_alternative_executable);
            p_standalone.Controls.Add(txt_alternative_executable);
            p_standalone.Controls.Add(rb_wait_alternative_executable);
            p_standalone.Controls.Add(rb_wait_executable);
            p_standalone.Controls.Add(txt_executable);
            p_standalone.Controls.Add(lbl_app_executable);
            p_standalone.Controls.Add(label2);
            p_standalone.Controls.Add(nud_timeout_executable);
            p_standalone.Enabled = false;
            p_standalone.Location = new System.Drawing.Point(4, 91);
            p_standalone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_standalone.Name = "p_standalone";
            p_standalone.Size = new System.Drawing.Size(1220, 232);
            p_standalone.TabIndex = 46;
            // 
            // cb_run_exe_as_administrator
            // 
            cb_run_exe_as_administrator.AutoSize = true;
            cb_run_exe_as_administrator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_run_exe_as_administrator.ForeColor = System.Drawing.Color.White;
            cb_run_exe_as_administrator.Location = new System.Drawing.Point(200, 93);
            cb_run_exe_as_administrator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_run_exe_as_administrator.Name = "cb_run_exe_as_administrator";
            cb_run_exe_as_administrator.Size = new System.Drawing.Size(256, 24);
            cb_run_exe_as_administrator.TabIndex = 39;
            cb_run_exe_as_administrator.Text = "Run executable as administrator";
            cb_run_exe_as_administrator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_run_exe_as_administrator.UseVisualStyleBackColor = true;
            cb_run_exe_as_administrator.Paint += checkbox_Paint;
            // 
            // btn_choose_exe_icon
            // 
            btn_choose_exe_icon.Enabled = false;
            btn_choose_exe_icon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_choose_exe_icon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_choose_exe_icon.ForeColor = System.Drawing.Color.White;
            btn_choose_exe_icon.Location = new System.Drawing.Point(42, 182);
            btn_choose_exe_icon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_choose_exe_icon.Name = "btn_choose_exe_icon";
            btn_choose_exe_icon.Size = new System.Drawing.Size(117, 30);
            btn_choose_exe_icon.TabIndex = 38;
            btn_choose_exe_icon.Text = "Swap";
            btn_choose_exe_icon.UseVisualStyleBackColor = true;
            btn_choose_exe_icon.Click += btn_choose_exe_icon_Click;
            // 
            // pb_exe_icon
            // 
            pb_exe_icon.BackColor = System.Drawing.Color.DarkGray;
            pb_exe_icon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pb_exe_icon.Location = new System.Drawing.Point(42, 68);
            pb_exe_icon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pb_exe_icon.Name = "pb_exe_icon";
            pb_exe_icon.Size = new System.Drawing.Size(116, 115);
            pb_exe_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_exe_icon.TabIndex = 37;
            pb_exe_icon.TabStop = false;
            pb_exe_icon.Click += pb_exe_icon_Click;
            // 
            // cbx_exe_priority
            // 
            cbx_exe_priority.AllowDrop = true;
            cbx_exe_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbx_exe_priority.FormattingEnabled = true;
            cbx_exe_priority.Location = new System.Drawing.Point(1036, 95);
            cbx_exe_priority.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbx_exe_priority.Name = "cbx_exe_priority";
            cbx_exe_priority.Size = new System.Drawing.Size(174, 28);
            cbx_exe_priority.TabIndex = 31;
            // 
            // lbl_exe_priority
            // 
            lbl_exe_priority.AutoSize = true;
            lbl_exe_priority.ForeColor = System.Drawing.Color.White;
            lbl_exe_priority.Location = new System.Drawing.Point(864, 98);
            lbl_exe_priority.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_exe_priority.Name = "lbl_exe_priority";
            lbl_exe_priority.Size = new System.Drawing.Size(143, 20);
            lbl_exe_priority.TabIndex = 30;
            lbl_exe_priority.Text = "Executable Priority:";
            lbl_exe_priority.Paint += label_Paint;
            // 
            // btn_exe_to_start
            // 
            btn_exe_to_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_exe_to_start.ForeColor = System.Drawing.Color.White;
            btn_exe_to_start.Location = new System.Drawing.Point(777, 12);
            btn_exe_to_start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_exe_to_start.Name = "btn_exe_to_start";
            btn_exe_to_start.Size = new System.Drawing.Size(99, 31);
            btn_exe_to_start.TabIndex = 12;
            btn_exe_to_start.Text = "Choose";
            btn_exe_to_start.UseVisualStyleBackColor = true;
            btn_exe_to_start.Click += btn_exe_to_start_Click;
            // 
            // txt_args_executable
            // 
            txt_args_executable.Enabled = false;
            txt_args_executable.Location = new System.Drawing.Point(496, 53);
            txt_args_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_args_executable.Name = "txt_args_executable";
            txt_args_executable.Size = new System.Drawing.Size(714, 26);
            txt_args_executable.TabIndex = 11;
            // 
            // cb_args_executable
            // 
            cb_args_executable.AutoSize = true;
            cb_args_executable.ForeColor = System.Drawing.Color.White;
            cb_args_executable.Location = new System.Drawing.Point(200, 55);
            cb_args_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_args_executable.Name = "cb_args_executable";
            cb_args_executable.Size = new System.Drawing.Size(248, 24);
            cb_args_executable.TabIndex = 10;
            cb_args_executable.Text = "Pass arguments to Executable:";
            cb_args_executable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_args_executable.UseVisualStyleBackColor = true;
            cb_args_executable.CheckedChanged += cb_args_executable_CheckedChanged;
            cb_args_executable.Paint += checkbox_Paint;
            // 
            // btn_choose_alternative_executable
            // 
            btn_choose_alternative_executable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_choose_alternative_executable.ForeColor = System.Drawing.Color.White;
            btn_choose_alternative_executable.Location = new System.Drawing.Point(1112, 179);
            btn_choose_alternative_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_choose_alternative_executable.Name = "btn_choose_alternative_executable";
            btn_choose_alternative_executable.Size = new System.Drawing.Size(99, 31);
            btn_choose_alternative_executable.TabIndex = 9;
            btn_choose_alternative_executable.Text = "Choose";
            btn_choose_alternative_executable.UseVisualStyleBackColor = true;
            btn_choose_alternative_executable.Click += btn_choose_alternative_executable_Click;
            // 
            // txt_alternative_executable
            // 
            txt_alternative_executable.Enabled = false;
            txt_alternative_executable.Location = new System.Drawing.Point(738, 180);
            txt_alternative_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_alternative_executable.Name = "txt_alternative_executable";
            txt_alternative_executable.Size = new System.Drawing.Size(366, 26);
            txt_alternative_executable.TabIndex = 4;
            txt_alternative_executable.TextChanged += txt_alternative_executable_TextChanged;
            // 
            // rb_wait_alternative_executable
            // 
            rb_wait_alternative_executable.AutoSize = true;
            rb_wait_alternative_executable.ForeColor = System.Drawing.Color.White;
            rb_wait_alternative_executable.Location = new System.Drawing.Point(197, 180);
            rb_wait_alternative_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_wait_alternative_executable.Name = "rb_wait_alternative_executable";
            rb_wait_alternative_executable.Size = new System.Drawing.Size(468, 24);
            rb_wait_alternative_executable.TabIndex = 8;
            rb_wait_alternative_executable.Text = "Wait until an alternative executable is closed before continuing:";
            rb_wait_alternative_executable.UseVisualStyleBackColor = true;
            rb_wait_alternative_executable.CheckedChanged += rb_wait_alternative_executable_CheckedChanged;
            rb_wait_alternative_executable.Paint += radiobutton_Paint;
            // 
            // rb_wait_executable
            // 
            rb_wait_executable.AutoSize = true;
            rb_wait_executable.Checked = true;
            rb_wait_executable.ForeColor = System.Drawing.Color.White;
            rb_wait_executable.Location = new System.Drawing.Point(197, 135);
            rb_wait_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_wait_executable.Name = "rb_wait_executable";
            rb_wait_executable.Size = new System.Drawing.Size(439, 24);
            rb_wait_executable.TabIndex = 7;
            rb_wait_executable.TabStop = true;
            rb_wait_executable.Text = "Wait until the executable above is closed before continuing";
            rb_wait_executable.UseVisualStyleBackColor = true;
            rb_wait_executable.CheckedChanged += rb_wait_executable_CheckedChanged;
            rb_wait_executable.Paint += radiobutton_Paint;
            // 
            // txt_executable
            // 
            txt_executable.Location = new System.Drawing.Point(200, 12);
            txt_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_executable.Name = "txt_executable";
            txt_executable.Size = new System.Drawing.Size(570, 26);
            txt_executable.TabIndex = 1;
            txt_executable.TextChanged += txt_executable_TextChanged;
            // 
            // lbl_app_executable
            // 
            lbl_app_executable.AutoSize = true;
            lbl_app_executable.ForeColor = System.Drawing.Color.White;
            lbl_app_executable.Location = new System.Drawing.Point(22, 15);
            lbl_app_executable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_app_executable.Name = "lbl_app_executable";
            lbl_app_executable.Size = new System.Drawing.Size(146, 20);
            lbl_app_executable.TabIndex = 0;
            lbl_app_executable.Text = "Executable to start:";
            lbl_app_executable.TextAlign = System.Drawing.ContentAlignment.TopRight;
            lbl_app_executable.Paint += label_Paint;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = System.Drawing.Color.Transparent;
            label2.Location = new System.Drawing.Point(994, 15);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(125, 20);
            label2.TabIndex = 5;
            label2.Text = "Max Wait (secs):";
            label2.Paint += label_Paint;
            // 
            // nud_timeout_executable
            // 
            nud_timeout_executable.Location = new System.Drawing.Point(1147, 12);
            nud_timeout_executable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_timeout_executable.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
            nud_timeout_executable.Name = "nud_timeout_executable";
            nud_timeout_executable.Size = new System.Drawing.Size(64, 26);
            nud_timeout_executable.TabIndex = 6;
            nud_timeout_executable.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // rb_standalone
            // 
            rb_standalone.AutoSize = true;
            rb_standalone.ForeColor = System.Drawing.Color.White;
            rb_standalone.Location = new System.Drawing.Point(19, 61);
            rb_standalone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_standalone.Name = "rb_standalone";
            rb_standalone.Size = new System.Drawing.Size(326, 24);
            rb_standalone.TabIndex = 45;
            rb_standalone.Text = "Launch a Game or Application executable ";
            rb_standalone.UseVisualStyleBackColor = true;
            rb_standalone.CheckedChanged += rb_standalone_CheckedChanged;
            rb_standalone.Paint += radiobutton_Paint;
            // 
            // rb_no_game
            // 
            rb_no_game.AutoSize = true;
            rb_no_game.ForeColor = System.Drawing.Color.White;
            rb_no_game.Location = new System.Drawing.Point(18, 13);
            rb_no_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_no_game.Name = "rb_no_game";
            rb_no_game.Size = new System.Drawing.Size(162, 24);
            rb_no_game.TabIndex = 44;
            rb_no_game.Text = "Don't start a Game";
            rb_no_game.UseVisualStyleBackColor = true;
            rb_no_game.CheckedChanged += rb_no_game_CheckedChanged;
            rb_no_game.Paint += radiobutton_Paint;
            // 
            // p_game
            // 
            p_game.Anchor = System.Windows.Forms.AnchorStyles.None;
            p_game.Controls.Add(btn_refresh_games_list);
            p_game.Controls.Add(btn_choose_game_icon);
            p_game.Controls.Add(pb_game_icon);
            p_game.Controls.Add(lbl_no_game_libraries);
            p_game.Controls.Add(cbx_game_priority);
            p_game.Controls.Add(cb_wait_alternative_game);
            p_game.Controls.Add(btn_choose_alternative_game);
            p_game.Controls.Add(txt_alternative_game);
            p_game.Controls.Add(txt_game_name);
            p_game.Controls.Add(lbl_game_priority);
            p_game.Controls.Add(lbl_game_name);
            p_game.Controls.Add(txt_args_game);
            p_game.Controls.Add(cb_args_game);
            p_game.Controls.Add(lbl_game_timeout);
            p_game.Controls.Add(nud_timeout_game);
            p_game.Controls.Add(lbl_game_library);
            p_game.Location = new System.Drawing.Point(4, 382);
            p_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_game.Name = "p_game";
            p_game.Size = new System.Drawing.Size(1220, 209);
            p_game.TabIndex = 43;
            // 
            // btn_refresh_games_list
            // 
            btn_refresh_games_list.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btn_refresh_games_list.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_refresh_games_list.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_refresh_games_list.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_refresh_games_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_refresh_games_list.ForeColor = System.Drawing.Color.White;
            btn_refresh_games_list.Location = new System.Drawing.Point(1074, 170);
            btn_refresh_games_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_refresh_games_list.Name = "btn_refresh_games_list";
            btn_refresh_games_list.Size = new System.Drawing.Size(136, 29);
            btn_refresh_games_list.TabIndex = 42;
            btn_refresh_games_list.Text = "Refresh Games List";
            btn_refresh_games_list.UseVisualStyleBackColor = true;
            btn_refresh_games_list.Click += btn_refresh_games_list_Click;
            // 
            // btn_choose_game_icon
            // 
            btn_choose_game_icon.Enabled = false;
            btn_choose_game_icon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_choose_game_icon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_choose_game_icon.ForeColor = System.Drawing.Color.White;
            btn_choose_game_icon.Location = new System.Drawing.Point(42, 167);
            btn_choose_game_icon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_choose_game_icon.Name = "btn_choose_game_icon";
            btn_choose_game_icon.Size = new System.Drawing.Size(117, 30);
            btn_choose_game_icon.TabIndex = 37;
            btn_choose_game_icon.Text = "Swap";
            btn_choose_game_icon.UseVisualStyleBackColor = true;
            btn_choose_game_icon.Click += btn_choose_game_icon_Click;
            // 
            // pb_game_icon
            // 
            pb_game_icon.BackColor = System.Drawing.Color.DarkGray;
            pb_game_icon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pb_game_icon.Location = new System.Drawing.Point(42, 55);
            pb_game_icon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pb_game_icon.Name = "pb_game_icon";
            pb_game_icon.Size = new System.Drawing.Size(116, 115);
            pb_game_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_game_icon.TabIndex = 35;
            pb_game_icon.TabStop = false;
            pb_game_icon.Click += pb_game_icon_Click;
            // 
            // lbl_no_game_libraries
            // 
            lbl_no_game_libraries.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_no_game_libraries.AutoSize = true;
            lbl_no_game_libraries.BackColor = System.Drawing.Color.Brown;
            lbl_no_game_libraries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_no_game_libraries.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_no_game_libraries.ForeColor = System.Drawing.Color.White;
            lbl_no_game_libraries.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_no_game_libraries.Location = new System.Drawing.Point(243, 170);
            lbl_no_game_libraries.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_no_game_libraries.Name = "lbl_no_game_libraries";
            lbl_no_game_libraries.Size = new System.Drawing.Size(613, 22);
            lbl_no_game_libraries.TabIndex = 34;
            lbl_no_game_libraries.Text = "No supported game libraries detected! (Steam, Origin, Uplay, Epic or GOG supported)";
            lbl_no_game_libraries.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_no_game_libraries.Visible = false;
            // 
            // cbx_game_priority
            // 
            cbx_game_priority.AllowDrop = true;
            cbx_game_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbx_game_priority.FormattingEnabled = true;
            cbx_game_priority.Location = new System.Drawing.Point(777, 14);
            cbx_game_priority.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbx_game_priority.Name = "cbx_game_priority";
            cbx_game_priority.Size = new System.Drawing.Size(191, 28);
            cbx_game_priority.TabIndex = 29;
            // 
            // cb_wait_alternative_game
            // 
            cb_wait_alternative_game.AutoSize = true;
            cb_wait_alternative_game.Location = new System.Drawing.Point(192, 115);
            cb_wait_alternative_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_wait_alternative_game.Name = "cb_wait_alternative_game";
            cb_wait_alternative_game.Size = new System.Drawing.Size(229, 24);
            cb_wait_alternative_game.TabIndex = 27;
            cb_wait_alternative_game.Text = "Monitor different executable:";
            cb_wait_alternative_game.UseVisualStyleBackColor = true;
            cb_wait_alternative_game.CheckedChanged += cb_wait_alternative_game_CheckedChanged;
            cb_wait_alternative_game.Paint += checkbox_Paint;
            // 
            // btn_choose_alternative_game
            // 
            btn_choose_alternative_game.Enabled = false;
            btn_choose_alternative_game.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_choose_alternative_game.ForeColor = System.Drawing.Color.White;
            btn_choose_alternative_game.Location = new System.Drawing.Point(1112, 113);
            btn_choose_alternative_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_choose_alternative_game.Name = "btn_choose_alternative_game";
            btn_choose_alternative_game.Size = new System.Drawing.Size(99, 31);
            btn_choose_alternative_game.TabIndex = 26;
            btn_choose_alternative_game.Text = "Choose";
            btn_choose_alternative_game.UseVisualStyleBackColor = true;
            btn_choose_alternative_game.Click += btn_choose_alternative_game_Click;
            // 
            // txt_alternative_game
            // 
            txt_alternative_game.Enabled = false;
            txt_alternative_game.Location = new System.Drawing.Point(465, 113);
            txt_alternative_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_alternative_game.Name = "txt_alternative_game";
            txt_alternative_game.Size = new System.Drawing.Size(639, 26);
            txt_alternative_game.TabIndex = 24;
            txt_alternative_game.TextChanged += txt_alternative_game_TextChanged;
            // 
            // txt_game_name
            // 
            txt_game_name.Enabled = false;
            txt_game_name.Location = new System.Drawing.Point(175, 13);
            txt_game_name.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_game_name.Name = "txt_game_name";
            txt_game_name.ReadOnly = true;
            txt_game_name.Size = new System.Drawing.Size(448, 26);
            txt_game_name.TabIndex = 21;
            txt_game_name.Text = "Please select a game from the list below...";
            // 
            // lbl_game_priority
            // 
            lbl_game_priority.AutoSize = true;
            lbl_game_priority.ForeColor = System.Drawing.Color.White;
            lbl_game_priority.Location = new System.Drawing.Point(650, 17);
            lbl_game_priority.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_game_priority.Name = "lbl_game_priority";
            lbl_game_priority.Size = new System.Drawing.Size(108, 20);
            lbl_game_priority.TabIndex = 18;
            lbl_game_priority.Text = "Game Priority:";
            lbl_game_priority.Paint += label_Paint;
            // 
            // lbl_game_name
            // 
            lbl_game_name.AutoSize = true;
            lbl_game_name.ForeColor = System.Drawing.Color.White;
            lbl_game_name.Location = new System.Drawing.Point(29, 16);
            lbl_game_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_game_name.Name = "lbl_game_name";
            lbl_game_name.Size = new System.Drawing.Size(124, 20);
            lbl_game_name.TabIndex = 17;
            lbl_game_name.Text = "Selected Game:";
            lbl_game_name.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lbl_game_name.Paint += label_Paint;
            // 
            // txt_args_game
            // 
            txt_args_game.Enabled = false;
            txt_args_game.Location = new System.Drawing.Point(465, 70);
            txt_args_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_args_game.Name = "txt_args_game";
            txt_args_game.Size = new System.Drawing.Size(745, 26);
            txt_args_game.TabIndex = 13;
            // 
            // cb_args_game
            // 
            cb_args_game.AutoSize = true;
            cb_args_game.ForeColor = System.Drawing.Color.White;
            cb_args_game.Location = new System.Drawing.Point(194, 73);
            cb_args_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_args_game.Name = "cb_args_game";
            cb_args_game.Size = new System.Drawing.Size(213, 24);
            cb_args_game.TabIndex = 12;
            cb_args_game.Text = "Pass arguments to Game:";
            cb_args_game.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_args_game.UseVisualStyleBackColor = true;
            cb_args_game.CheckedChanged += cb_args_game_CheckedChanged;
            cb_args_game.Paint += checkbox_Paint;
            // 
            // lbl_game_timeout
            // 
            lbl_game_timeout.AutoSize = true;
            lbl_game_timeout.ForeColor = System.Drawing.Color.White;
            lbl_game_timeout.Location = new System.Drawing.Point(995, 16);
            lbl_game_timeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_game_timeout.Name = "lbl_game_timeout";
            lbl_game_timeout.Size = new System.Drawing.Size(125, 20);
            lbl_game_timeout.TabIndex = 4;
            lbl_game_timeout.Text = "Max Wait (secs):";
            lbl_game_timeout.Paint += label_Paint;
            // 
            // nud_timeout_game
            // 
            nud_timeout_game.Location = new System.Drawing.Point(1148, 15);
            nud_timeout_game.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_timeout_game.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
            nud_timeout_game.Name = "nud_timeout_game";
            nud_timeout_game.Size = new System.Drawing.Size(63, 26);
            nud_timeout_game.TabIndex = 5;
            nud_timeout_game.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lbl_game_library
            // 
            lbl_game_library.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_game_library.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_game_library.ForeColor = System.Drawing.Color.White;
            lbl_game_library.Location = new System.Drawing.Point(476, 42);
            lbl_game_library.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_game_library.Name = "lbl_game_library";
            lbl_game_library.Size = new System.Drawing.Size(148, 25);
            lbl_game_library.TabIndex = 30;
            lbl_game_library.Text = "Game Library:";
            lbl_game_library.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // rb_launcher
            // 
            rb_launcher.AutoSize = true;
            rb_launcher.Checked = true;
            rb_launcher.ForeColor = System.Drawing.Color.White;
            rb_launcher.Location = new System.Drawing.Point(18, 348);
            rb_launcher.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_launcher.Name = "rb_launcher";
            rb_launcher.Size = new System.Drawing.Size(466, 24);
            rb_launcher.TabIndex = 42;
            rb_launcher.TabStop = true;
            rb_launcher.Text = "Launch a Game installed in Steam, Origin, Uplay, Epic or GOG";
            rb_launcher.UseVisualStyleBackColor = true;
            rb_launcher.CheckedChanged += rb_launcher_CheckedChanged;
            rb_launcher.Paint += radiobutton_Paint;
            // 
            // tabp_after
            // 
            tabp_after.BackColor = System.Drawing.Color.Black;
            tabp_after.Controls.Add(groupBox3);
            tabp_after.Controls.Add(groupBox2);
            tabp_after.Controls.Add(groupBox1);
            tabp_after.Controls.Add(gb_display_after);
            tabp_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabp_after.ForeColor = System.Drawing.Color.White;
            tabp_after.Location = new System.Drawing.Point(4, 32);
            tabp_after.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_after.Name = "tabp_after";
            tabp_after.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_after.Size = new System.Drawing.Size(1229, 849);
            tabp_after.TabIndex = 3;
            tabp_after.Text = "5. Choose what happens afterwards";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            groupBox3.Controls.Add(cb_run_cmd_afterwards_run_as_administrator);
            groupBox3.Controls.Add(cb_run_cmd_afterwards_dont_start);
            groupBox3.Controls.Add(txt_run_cmd_afterwards_args);
            groupBox3.Controls.Add(cb_run_cmd_afterwards_args);
            groupBox3.Controls.Add(btn_run_cmd_afterwards);
            groupBox3.Controls.Add(txt_run_cmd_afterwards);
            groupBox3.Controls.Add(cb_run_cmd_afterwards);
            groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox3.ForeColor = System.Drawing.Color.White;
            groupBox3.Location = new System.Drawing.Point(204, 628);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(892, 181);
            groupBox3.TabIndex = 14;
            groupBox3.TabStop = false;
            groupBox3.Text = "Run a program or command afterwards?";
            // 
            // cb_run_cmd_afterwards_run_as_administrator
            // 
            cb_run_cmd_afterwards_run_as_administrator.AutoSize = true;
            cb_run_cmd_afterwards_run_as_administrator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_run_cmd_afterwards_run_as_administrator.ForeColor = System.Drawing.Color.White;
            cb_run_cmd_afterwards_run_as_administrator.Location = new System.Drawing.Point(574, 132);
            cb_run_cmd_afterwards_run_as_administrator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_run_cmd_afterwards_run_as_administrator.Name = "cb_run_cmd_afterwards_run_as_administrator";
            cb_run_cmd_afterwards_run_as_administrator.Size = new System.Drawing.Size(238, 24);
            cb_run_cmd_afterwards_run_as_administrator.TabIndex = 36;
            cb_run_cmd_afterwards_run_as_administrator.Text = "Run program as administrator";
            cb_run_cmd_afterwards_run_as_administrator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_run_cmd_afterwards_run_as_administrator.UseVisualStyleBackColor = true;
            cb_run_cmd_afterwards_run_as_administrator.Paint += checkbox_Paint;
            // 
            // cb_run_cmd_afterwards_dont_start
            // 
            cb_run_cmd_afterwards_dont_start.AutoSize = true;
            cb_run_cmd_afterwards_dont_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_run_cmd_afterwards_dont_start.ForeColor = System.Drawing.Color.White;
            cb_run_cmd_afterwards_dont_start.Location = new System.Drawing.Point(114, 132);
            cb_run_cmd_afterwards_dont_start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_run_cmd_afterwards_dont_start.Name = "cb_run_cmd_afterwards_dont_start";
            cb_run_cmd_afterwards_dont_start.Size = new System.Drawing.Size(289, 24);
            cb_run_cmd_afterwards_dont_start.TabIndex = 27;
            cb_run_cmd_afterwards_dont_start.Text = "Don't start if program already running";
            cb_run_cmd_afterwards_dont_start.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_run_cmd_afterwards_dont_start.UseVisualStyleBackColor = true;
            cb_run_cmd_afterwards_dont_start.Paint += checkbox_Paint;
            // 
            // txt_run_cmd_afterwards_args
            // 
            txt_run_cmd_afterwards_args.Enabled = false;
            txt_run_cmd_afterwards_args.Location = new System.Drawing.Point(293, 87);
            txt_run_cmd_afterwards_args.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_run_cmd_afterwards_args.Name = "txt_run_cmd_afterwards_args";
            txt_run_cmd_afterwards_args.Size = new System.Drawing.Size(558, 26);
            txt_run_cmd_afterwards_args.TabIndex = 13;
            // 
            // cb_run_cmd_afterwards_args
            // 
            cb_run_cmd_afterwards_args.AutoSize = true;
            cb_run_cmd_afterwards_args.ForeColor = System.Drawing.Color.White;
            cb_run_cmd_afterwards_args.Location = new System.Drawing.Point(114, 89);
            cb_run_cmd_afterwards_args.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_run_cmd_afterwards_args.Name = "cb_run_cmd_afterwards_args";
            cb_run_cmd_afterwards_args.Size = new System.Drawing.Size(147, 24);
            cb_run_cmd_afterwards_args.TabIndex = 12;
            cb_run_cmd_afterwards_args.Text = "Pass arguments:";
            cb_run_cmd_afterwards_args.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_run_cmd_afterwards_args.UseVisualStyleBackColor = true;
            cb_run_cmd_afterwards_args.CheckedChanged += cb_run_cmd_afterwards_args_CheckedChanged;
            cb_run_cmd_afterwards_args.Paint += checkbox_Paint;
            // 
            // btn_run_cmd_afterwards
            // 
            btn_run_cmd_afterwards.Enabled = false;
            btn_run_cmd_afterwards.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_run_cmd_afterwards.ForeColor = System.Drawing.Color.White;
            btn_run_cmd_afterwards.Location = new System.Drawing.Point(752, 40);
            btn_run_cmd_afterwards.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_run_cmd_afterwards.Name = "btn_run_cmd_afterwards";
            btn_run_cmd_afterwards.Size = new System.Drawing.Size(99, 31);
            btn_run_cmd_afterwards.TabIndex = 11;
            btn_run_cmd_afterwards.Text = "Choose";
            btn_run_cmd_afterwards.UseVisualStyleBackColor = true;
            btn_run_cmd_afterwards.Click += btn_run_cmd_afterwards_Click;
            // 
            // txt_run_cmd_afterwards
            // 
            txt_run_cmd_afterwards.Enabled = false;
            txt_run_cmd_afterwards.Location = new System.Drawing.Point(292, 42);
            txt_run_cmd_afterwards.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_run_cmd_afterwards.Name = "txt_run_cmd_afterwards";
            txt_run_cmd_afterwards.Size = new System.Drawing.Size(453, 26);
            txt_run_cmd_afterwards.TabIndex = 10;
            // 
            // cb_run_cmd_afterwards
            // 
            cb_run_cmd_afterwards.AutoSize = true;
            cb_run_cmd_afterwards.Location = new System.Drawing.Point(114, 44);
            cb_run_cmd_afterwards.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_run_cmd_afterwards.Name = "cb_run_cmd_afterwards";
            cb_run_cmd_afterwards.Size = new System.Drawing.Size(154, 24);
            cb_run_cmd_afterwards.TabIndex = 0;
            cb_run_cmd_afterwards.Text = "Run this program:";
            cb_run_cmd_afterwards.UseVisualStyleBackColor = true;
            cb_run_cmd_afterwards.CheckedChanged += cb_run_cmd_afterwards_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            groupBox2.Controls.Add(rb_switch_capture_permanent);
            groupBox2.Controls.Add(rb_switch_capture_temp);
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox2.ForeColor = System.Drawing.Color.White;
            groupBox2.Location = new System.Drawing.Point(204, 425);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(892, 173);
            groupBox2.TabIndex = 13;
            groupBox2.TabStop = false;
            groupBox2.Text = "What happens to the Microphone afterwards?";
            // 
            // rb_switch_capture_permanent
            // 
            rb_switch_capture_permanent.AutoSize = true;
            rb_switch_capture_permanent.ForeColor = System.Drawing.Color.White;
            rb_switch_capture_permanent.Location = new System.Drawing.Point(114, 106);
            rb_switch_capture_permanent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_capture_permanent.Name = "rb_switch_capture_permanent";
            rb_switch_capture_permanent.Size = new System.Drawing.Size(492, 24);
            rb_switch_capture_permanent.TabIndex = 12;
            rb_switch_capture_permanent.Text = "Keep using the Microphone after Game ends (permanent change)";
            rb_switch_capture_permanent.UseVisualStyleBackColor = true;
            rb_switch_capture_permanent.CheckedChanged += rb_switch_capture_permanent_CheckedChanged;
            // 
            // rb_switch_capture_temp
            // 
            rb_switch_capture_temp.AutoSize = true;
            rb_switch_capture_temp.Checked = true;
            rb_switch_capture_temp.ForeColor = System.Drawing.Color.White;
            rb_switch_capture_temp.Location = new System.Drawing.Point(114, 51);
            rb_switch_capture_temp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_capture_temp.Name = "rb_switch_capture_temp";
            rb_switch_capture_temp.Size = new System.Drawing.Size(553, 24);
            rb_switch_capture_temp.TabIndex = 11;
            rb_switch_capture_temp.TabStop = true;
            rb_switch_capture_temp.Text = "Revert back to original Microphone (temporary change while running game)";
            rb_switch_capture_temp.UseVisualStyleBackColor = true;
            rb_switch_capture_temp.CheckedChanged += rb_switch_capture_temp_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            groupBox1.Controls.Add(rb_switch_audio_permanent);
            groupBox1.Controls.Add(rb_switch_audio_temp);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox1.ForeColor = System.Drawing.Color.White;
            groupBox1.Location = new System.Drawing.Point(204, 220);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(892, 173);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "What happens to the Audio output afterwards?";
            // 
            // rb_switch_audio_permanent
            // 
            rb_switch_audio_permanent.AutoSize = true;
            rb_switch_audio_permanent.ForeColor = System.Drawing.Color.White;
            rb_switch_audio_permanent.Location = new System.Drawing.Point(114, 106);
            rb_switch_audio_permanent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_audio_permanent.Name = "rb_switch_audio_permanent";
            rb_switch_audio_permanent.Size = new System.Drawing.Size(502, 24);
            rb_switch_audio_permanent.TabIndex = 12;
            rb_switch_audio_permanent.Text = "Keep using the Audio Device after Game ends (permanent change)";
            rb_switch_audio_permanent.UseVisualStyleBackColor = true;
            rb_switch_audio_permanent.CheckedChanged += rb_switch_audio_permanent_CheckedChanged;
            // 
            // rb_switch_audio_temp
            // 
            rb_switch_audio_temp.AutoSize = true;
            rb_switch_audio_temp.Checked = true;
            rb_switch_audio_temp.ForeColor = System.Drawing.Color.White;
            rb_switch_audio_temp.Location = new System.Drawing.Point(114, 51);
            rb_switch_audio_temp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_audio_temp.Name = "rb_switch_audio_temp";
            rb_switch_audio_temp.Size = new System.Drawing.Size(563, 24);
            rb_switch_audio_temp.TabIndex = 11;
            rb_switch_audio_temp.TabStop = true;
            rb_switch_audio_temp.Text = "Revert back to original Audio Device (temporary change while running game)";
            rb_switch_audio_temp.UseVisualStyleBackColor = true;
            rb_switch_audio_temp.CheckedChanged += rb_switch_audio_temp_CheckedChanged;
            // 
            // gb_display_after
            // 
            gb_display_after.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_display_after.Controls.Add(rb_switch_display_permanent);
            gb_display_after.Controls.Add(rb_switch_display_temp);
            gb_display_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_display_after.ForeColor = System.Drawing.Color.White;
            gb_display_after.Location = new System.Drawing.Point(204, 25);
            gb_display_after.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_display_after.Name = "gb_display_after";
            gb_display_after.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_display_after.Size = new System.Drawing.Size(892, 168);
            gb_display_after.TabIndex = 11;
            gb_display_after.TabStop = false;
            gb_display_after.Text = "What happens to the Display Profile afterwards?";
            // 
            // rb_switch_display_permanent
            // 
            rb_switch_display_permanent.AutoSize = true;
            rb_switch_display_permanent.ForeColor = System.Drawing.Color.White;
            rb_switch_display_permanent.Location = new System.Drawing.Point(114, 106);
            rb_switch_display_permanent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_display_permanent.Name = "rb_switch_display_permanent";
            rb_switch_display_permanent.Size = new System.Drawing.Size(508, 24);
            rb_switch_display_permanent.TabIndex = 12;
            rb_switch_display_permanent.Text = "Keep using the Display Profile after Game ends (permanent change)";
            rb_switch_display_permanent.UseVisualStyleBackColor = true;
            rb_switch_display_permanent.CheckedChanged += rb_switch_display_permanent_CheckedChanged;
            // 
            // rb_switch_display_temp
            // 
            rb_switch_display_temp.AutoSize = true;
            rb_switch_display_temp.Checked = true;
            rb_switch_display_temp.ForeColor = System.Drawing.Color.White;
            rb_switch_display_temp.Location = new System.Drawing.Point(114, 51);
            rb_switch_display_temp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_switch_display_temp.Name = "rb_switch_display_temp";
            rb_switch_display_temp.Size = new System.Drawing.Size(569, 24);
            rb_switch_display_temp.TabIndex = 11;
            rb_switch_display_temp.TabStop = true;
            rb_switch_display_temp.Text = "Revert back to original Display Profile (temporary change while running game)";
            rb_switch_display_temp.UseVisualStyleBackColor = true;
            rb_switch_display_temp.CheckedChanged += rb_switch_display_temp_CheckedChanged;
            // 
            // txt_shortcut_save_name
            // 
            txt_shortcut_save_name.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txt_shortcut_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_shortcut_save_name.Location = new System.Drawing.Point(241, 974);
            txt_shortcut_save_name.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_shortcut_save_name.MaxLength = 200;
            txt_shortcut_save_name.Name = "txt_shortcut_save_name";
            txt_shortcut_save_name.Size = new System.Drawing.Size(832, 35);
            txt_shortcut_save_name.TabIndex = 29;
            txt_shortcut_save_name.Click += txt_shortcut_save_name_Click;
            txt_shortcut_save_name.TextChanged += txt_shortcut_save_name_TextChanged;
            // 
            // lbl_title
            // 
            lbl_title.AutoSize = true;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(449, 16);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(345, 33);
            lbl_title.TabIndex = 30;
            lbl_title.Text = "Configure Game Shortcut";
            // 
            // lbl_shortcut_name
            // 
            lbl_shortcut_name.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_shortcut_name.AutoSize = true;
            lbl_shortcut_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_shortcut_name.ForeColor = System.Drawing.Color.Transparent;
            lbl_shortcut_name.Location = new System.Drawing.Point(27, 977);
            lbl_shortcut_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_shortcut_name.Name = "lbl_shortcut_name";
            lbl_shortcut_name.Size = new System.Drawing.Size(178, 29);
            lbl_shortcut_name.TabIndex = 31;
            lbl_shortcut_name.Text = "Shortcut Name:";
            // 
            // cb_autosuggest
            // 
            cb_autosuggest.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cb_autosuggest.Checked = true;
            cb_autosuggest.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_autosuggest.ForeColor = System.Drawing.Color.White;
            cb_autosuggest.Location = new System.Drawing.Point(1113, 984);
            cb_autosuggest.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_autosuggest.Name = "cb_autosuggest";
            cb_autosuggest.Size = new System.Drawing.Size(136, 20);
            cb_autosuggest.TabIndex = 32;
            cb_autosuggest.Text = "Auto-suggest name";
            cb_autosuggest.UseVisualStyleBackColor = true;
            cb_autosuggest.CheckedChanged += cb_autosuggest_CheckedChanged;
            // 
            // btn_hotkey
            // 
            btn_hotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_hotkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_hotkey.ForeColor = System.Drawing.Color.White;
            btn_hotkey.Location = new System.Drawing.Point(489, 1028);
            btn_hotkey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey.Name = "btn_hotkey";
            btn_hotkey.Size = new System.Drawing.Size(140, 35);
            btn_hotkey.TabIndex = 36;
            btn_hotkey.Text = "&Hotkey";
            btn_hotkey.UseVisualStyleBackColor = true;
            btn_hotkey.Click += btn_hotkey_Click;
            // 
            // lbl_hotkey_assigned
            // 
            lbl_hotkey_assigned.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lbl_hotkey_assigned.AutoSize = true;
            lbl_hotkey_assigned.BackColor = System.Drawing.Color.Transparent;
            lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_assigned.Location = new System.Drawing.Point(30, 1047);
            lbl_hotkey_assigned.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            lbl_hotkey_assigned.Size = new System.Drawing.Size(56, 16);
            lbl_hotkey_assigned.TabIndex = 37;
            lbl_hotkey_assigned.Text = "Hotkey: ";
            lbl_hotkey_assigned.Visible = false;
            lbl_hotkey_assigned.Click += lbl_hotkey_assigned_Click;
            // 
            // btn_help
            // 
            btn_help.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_help.Location = new System.Drawing.Point(1163, 16);
            btn_help.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_help.Name = "btn_help";
            btn_help.Size = new System.Drawing.Size(88, 27);
            btn_help.TabIndex = 38;
            btn_help.Text = "&Help";
            btn_help.UseVisualStyleBackColor = true;
            btn_help.Click += btn_help_Click;
            // 
            // ShortcutForm
            // 
            AcceptButton = btn_save;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_cancel;
            ClientSize = new System.Drawing.Size(1265, 1088);
            Controls.Add(btn_help);
            Controls.Add(lbl_hotkey_assigned);
            Controls.Add(btn_hotkey);
            Controls.Add(cb_autosuggest);
            Controls.Add(txt_shortcut_save_name);
            Controls.Add(lbl_shortcut_name);
            Controls.Add(lbl_title);
            Controls.Add(tabc_shortcut);
            Controls.Add(btn_cancel);
            Controls.Add(btn_save);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1281, 1030);
            Name = "ShortcutForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician - Configure a Game Shortcut";
            FormClosing += ShortcutForm_FormClosing;
            Load += ShortcutForm_Load;
            tabc_shortcut.ResumeLayout(false);
            tabp_display.ResumeLayout(false);
            p_profiles.ResumeLayout(false);
            p_profiles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
            tabp_audio.ResumeLayout(false);
            tabp_audio.PerformLayout();
            gb_capture_settings.ResumeLayout(false);
            gb_capture_settings.PerformLayout();
            gb_capture_volume.ResumeLayout(false);
            gb_capture_volume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_capture_volume).EndInit();
            gb_audio_settings.ResumeLayout(false);
            gb_audio_settings.PerformLayout();
            gb_audio_volume.ResumeLayout(false);
            gb_audio_volume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_audio_volume).EndInit();
            tabp_before.ResumeLayout(false);
            p_start_program_upper.ResumeLayout(false);
            p_start_program_upper.PerformLayout();
            tabp_game.ResumeLayout(false);
            p_game_list.ResumeLayout(false);
            p_gametostart.ResumeLayout(false);
            p_gametostart.PerformLayout();
            p_standalone.ResumeLayout(false);
            p_standalone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_exe_icon).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_timeout_executable).EndInit();
            p_game.ResumeLayout(false);
            p_game.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_game_icon).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_timeout_game).EndInit();
            tabp_after.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            gb_display_after.ResumeLayout(false);
            gb_display_after.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.TabPage tabp_game;
        private System.Windows.Forms.TabPage tabp_after;
        private System.Windows.Forms.TextBox txt_shortcut_save_name;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_shortcut_name;
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
        private System.Windows.Forms.Label lbl_disabled_shortcut_audio_chipset;
        private System.Windows.Forms.Label lbl_no_active_audio_devices;
        private System.Windows.Forms.Label lbl_no_active_capture_devices;
        private System.Windows.Forms.Button btn_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_assigned;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btn_run_cmd_afterwards;
        private System.Windows.Forms.TextBox txt_run_cmd_afterwards;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards;
        private System.Windows.Forms.TextBox txt_run_cmd_afterwards_args;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards_args;
        private System.Windows.Forms.TabPage tabp_before;
        private System.Windows.Forms.Panel p_start_program_upper;
        private System.Windows.Forms.Button btn_find_examples_startprograms;
        private System.Windows.Forms.Button btn_add_new_start_program;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flp_start_programs;
        private System.Windows.Forms.Panel p_gametostart;
        private System.Windows.Forms.Button btn_find_examples_game;
        private System.Windows.Forms.Panel p_standalone;
        private System.Windows.Forms.Button btn_choose_exe_icon;
        private System.Windows.Forms.PictureBox pb_exe_icon;
        private System.Windows.Forms.ComboBox cbx_exe_priority;
        private System.Windows.Forms.Label lbl_exe_priority;
        private System.Windows.Forms.Button btn_exe_to_start;
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
        private System.Windows.Forms.RadioButton rb_no_game;
        private System.Windows.Forms.Panel p_game;
        private System.Windows.Forms.Button btn_refresh_games_list;
        private System.Windows.Forms.Button btn_choose_game_icon;
        private System.Windows.Forms.PictureBox pb_game_icon;
        private System.Windows.Forms.Label lbl_no_game_libraries;
        private System.Windows.Forms.ComboBox cbx_game_priority;
        private System.Windows.Forms.CheckBox cb_wait_alternative_game;
        private System.Windows.Forms.Button btn_choose_alternative_game;
        private System.Windows.Forms.TextBox txt_alternative_game;
        private System.Windows.Forms.TextBox txt_game_name;
        private System.Windows.Forms.Label lbl_game_priority;
        private System.Windows.Forms.Label lbl_game_name;
        private System.Windows.Forms.TextBox txt_args_game;
        private System.Windows.Forms.CheckBox cb_args_game;
        private System.Windows.Forms.Label lbl_game_timeout;
        private System.Windows.Forms.NumericUpDown nud_timeout_game;
        private System.Windows.Forms.Label lbl_game_library;
        private System.Windows.Forms.RadioButton rb_launcher;
        private System.Windows.Forms.Panel p_profiles;
        private System.Windows.Forms.Panel p_game_list;
        private Manina.Windows.Forms.ImageListView ilv_games;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards_dont_start;
        private System.Windows.Forms.CheckBox cb_run_exe_as_administrator;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards_run_as_administrator;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.CheckBox cb_audio_comms_device;
        private System.Windows.Forms.CheckBox cb_capture_comms_device;
    }
}