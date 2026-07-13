//using DisplayMagician.Resources;
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
            gb_audio_profile = new System.Windows.Forms.GroupBox();
            gb_audio_overrides = new System.Windows.Forms.GroupBox();
            txt_audio_profile_settings = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            nud_speaker_volume = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            nud_microphone_volume = new System.Windows.Forms.NumericUpDown();
            cb_override_microphone_volume = new System.Windows.Forms.CheckBox();
            cb_override_speaker_volume = new System.Windows.Forms.CheckBox();
            lbl_audio_profiles = new System.Windows.Forms.Label();
            lb_audio_profiles = new System.Windows.Forms.ListBox();
            btn_delete_audio_profile = new System.Windows.Forms.Button();
            btn_create_audio_profile = new System.Windows.Forms.Button();
            btn_update_audio_profile = new System.Windows.Forms.Button();
            cb_dont_change_audio = new System.Windows.Forms.CheckBox();
            tabp_before = new System.Windows.Forms.TabPage();
            flp_start_programs = new System.Windows.Forms.FlowLayoutPanel();
            p_start_program_upper = new System.Windows.Forms.Panel();
            btn_find_examples_startprograms = new System.Windows.Forms.Button();
            btn_add_new_stop_program = new System.Windows.Forms.Button();
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
            gb_audio_profile.SuspendLayout();
            gb_audio_overrides.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_speaker_volume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nud_microphone_volume).BeginInit();
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
            btn_save.Location = new System.Drawing.Point(654, 928);
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
            btn_cancel.Location = new System.Drawing.Point(1176, 936);
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
            tabc_shortcut.Location = new System.Drawing.Point(14, 58);
            tabc_shortcut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabc_shortcut.Name = "tabc_shortcut";
            tabc_shortcut.SelectedIndex = 0;
            tabc_shortcut.ShowToolTips = true;
            tabc_shortcut.Size = new System.Drawing.Size(1272, 786);
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
            tabp_display.Size = new System.Drawing.Size(1264, 750);
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
            ilv_saved_profiles.Size = new System.Drawing.Size(1256, 196);
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
            p_profiles.Size = new System.Drawing.Size(1256, 548);
            p_profiles.TabIndex = 39;
            // 
            // dv_profile
            // 
            dv_profile.BackColor = System.Drawing.Color.DimGray;
            dv_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            dv_profile.Dock = System.Windows.Forms.DockStyle.Fill;
            dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            dv_profile.Location = new System.Drawing.Point(0, 0);
            dv_profile.Margin = new System.Windows.Forms.Padding(21);
            dv_profile.Name = "dv_profile";
            dv_profile.Size = new System.Drawing.Size(1256, 548);
            dv_profile.TabIndex = 23;
            // 
            // pbLogo
            // 
            pbLogo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            pbLogo.BackColor = System.Drawing.Color.DimGray;
            pbLogo.Location = new System.Drawing.Point(1084, 30);
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
            tabp_audio.Controls.Add(gb_audio_profile);
            tabp_audio.Controls.Add(cb_dont_change_audio);
            tabp_audio.Location = new System.Drawing.Point(4, 32);
            tabp_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_audio.Name = "tabp_audio";
            tabp_audio.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_audio.Size = new System.Drawing.Size(1264, 750);
            tabp_audio.TabIndex = 4;
            tabp_audio.Text = "2. Choose Audio";
            // 
            // gb_audio_profile
            // 
            gb_audio_profile.Controls.Add(gb_audio_overrides);
            gb_audio_profile.Controls.Add(lbl_audio_profiles);
            gb_audio_profile.Controls.Add(lb_audio_profiles);
            gb_audio_profile.Controls.Add(btn_delete_audio_profile);
            gb_audio_profile.Controls.Add(btn_create_audio_profile);
            gb_audio_profile.Controls.Add(btn_update_audio_profile);
            gb_audio_profile.Enabled = false;
            gb_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_audio_profile.ForeColor = System.Drawing.Color.White;
            gb_audio_profile.Location = new System.Drawing.Point(26, 61);
            gb_audio_profile.Name = "gb_audio_profile";
            gb_audio_profile.Size = new System.Drawing.Size(1215, 664);
            gb_audio_profile.TabIndex = 2;
            gb_audio_profile.TabStop = false;
            gb_audio_profile.Text = "Audio Profiles to use";
            gb_audio_profile.Paint += groupbox_Paint;
            // 
            // gb_audio_overrides
            // 
            gb_audio_overrides.Controls.Add(txt_audio_profile_settings);
            gb_audio_overrides.Controls.Add(label4);
            gb_audio_overrides.Controls.Add(nud_speaker_volume);
            gb_audio_overrides.Controls.Add(label1);
            gb_audio_overrides.Controls.Add(nud_microphone_volume);
            gb_audio_overrides.Controls.Add(cb_override_microphone_volume);
            gb_audio_overrides.Controls.Add(cb_override_speaker_volume);
            gb_audio_overrides.Enabled = false;
            gb_audio_overrides.ForeColor = System.Drawing.Color.White;
            gb_audio_overrides.Location = new System.Drawing.Point(463, 55);
            gb_audio_overrides.Name = "gb_audio_overrides";
            gb_audio_overrides.Size = new System.Drawing.Size(703, 557);
            gb_audio_overrides.TabIndex = 5;
            gb_audio_overrides.TabStop = false;
            gb_audio_overrides.Text = "Selected Audio Profile Settings";
            gb_audio_overrides.Paint += groupbox_Paint;
            // 
            // txt_audio_profile_settings
            // 
            txt_audio_profile_settings.AcceptsReturn = true;
            txt_audio_profile_settings.AcceptsTab = true;
            txt_audio_profile_settings.Enabled = false;
            txt_audio_profile_settings.Location = new System.Drawing.Point(24, 32);
            txt_audio_profile_settings.Multiline = true;
            txt_audio_profile_settings.Name = "txt_audio_profile_settings";
            txt_audio_profile_settings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txt_audio_profile_settings.Size = new System.Drawing.Size(651, 453);
            txt_audio_profile_settings.TabIndex = 10;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = System.Drawing.Color.White;
            label4.Location = new System.Drawing.Point(652, 509);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(23, 20);
            label4.TabIndex = 9;
            label4.Text = "%";
            // 
            // nud_speaker_volume
            // 
            nud_speaker_volume.Enabled = false;
            nud_speaker_volume.Location = new System.Drawing.Point(240, 507);
            nud_speaker_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_speaker_volume.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nud_speaker_volume.Name = "nud_speaker_volume";
            nud_speaker_volume.Size = new System.Drawing.Size(50, 26);
            nud_speaker_volume.TabIndex = 6;
            nud_speaker_volume.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(293, 509);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(23, 20);
            label1.TabIndex = 8;
            label1.Text = "%";
            // 
            // nud_microphone_volume
            // 
            nud_microphone_volume.Enabled = false;
            nud_microphone_volume.Location = new System.Drawing.Point(600, 507);
            nud_microphone_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            nud_microphone_volume.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nud_microphone_volume.Name = "nud_microphone_volume";
            nud_microphone_volume.Size = new System.Drawing.Size(50, 26);
            nud_microphone_volume.TabIndex = 7;
            nud_microphone_volume.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // cb_override_microphone_volume
            // 
            cb_override_microphone_volume.AutoSize = true;
            cb_override_microphone_volume.ForeColor = System.Drawing.Color.White;
            cb_override_microphone_volume.Location = new System.Drawing.Point(360, 507);
            cb_override_microphone_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_override_microphone_volume.Name = "cb_override_microphone_volume";
            cb_override_microphone_volume.Size = new System.Drawing.Size(232, 24);
            cb_override_microphone_volume.TabIndex = 4;
            cb_override_microphone_volume.Text = "Override Microphone Volume";
            cb_override_microphone_volume.UseVisualStyleBackColor = true;
            cb_override_microphone_volume.CheckedChanged += cb_override_microphone_volume_CheckedChanged;
            // 
            // cb_override_speaker_volume
            // 
            cb_override_speaker_volume.AutoSize = true;
            cb_override_speaker_volume.ForeColor = System.Drawing.Color.White;
            cb_override_speaker_volume.Location = new System.Drawing.Point(24, 507);
            cb_override_speaker_volume.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_override_speaker_volume.Name = "cb_override_speaker_volume";
            cb_override_speaker_volume.Size = new System.Drawing.Size(209, 24);
            cb_override_speaker_volume.TabIndex = 3;
            cb_override_speaker_volume.Text = "Override Speaker Volume";
            cb_override_speaker_volume.UseVisualStyleBackColor = true;
            cb_override_speaker_volume.CheckedChanged += cb_override_speaker_volume_CheckedChanged;
            // 
            // lbl_audio_profiles
            // 
            lbl_audio_profiles.AutoSize = true;
            lbl_audio_profiles.ForeColor = System.Drawing.Color.White;
            lbl_audio_profiles.Location = new System.Drawing.Point(57, 55);
            lbl_audio_profiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_audio_profiles.Name = "lbl_audio_profiles";
            lbl_audio_profiles.Size = new System.Drawing.Size(338, 20);
            lbl_audio_profiles.TabIndex = 0;
            lbl_audio_profiles.Text = "Select an Audio Profile to use for this Shortcut:";
            lbl_audio_profiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_audio_profiles.Paint += label_Paint;
            // 
            // lb_audio_profiles
            // 
            lb_audio_profiles.BackColor = System.Drawing.Color.White;
            lb_audio_profiles.ForeColor = System.Drawing.Color.Black;
            lb_audio_profiles.FormattingEnabled = true;
            lb_audio_profiles.Location = new System.Drawing.Point(26, 76);
            lb_audio_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lb_audio_profiles.Name = "lb_audio_profiles";
            lb_audio_profiles.Size = new System.Drawing.Size(392, 384);
            lb_audio_profiles.TabIndex = 1;
            lb_audio_profiles.SelectedIndexChanged += lb_audio_profiles_SelectedIndexChanged;
            // 
            // btn_delete_audio_profile
            // 
            btn_delete_audio_profile.Enabled = false;
            btn_delete_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_delete_audio_profile.Location = new System.Drawing.Point(74, 581);
            btn_delete_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_delete_audio_profile.Name = "btn_delete_audio_profile";
            btn_delete_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_delete_audio_profile.TabIndex = 4;
            btn_delete_audio_profile.Text = "Delete Selected Profile";
            btn_delete_audio_profile.UseVisualStyleBackColor = true;
            btn_delete_audio_profile.Click += btn_delete_audio_profile_Click;
            // 
            // btn_create_audio_profile
            // 
            btn_create_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_create_audio_profile.Location = new System.Drawing.Point(74, 491);
            btn_create_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_create_audio_profile.Name = "btn_create_audio_profile";
            btn_create_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_create_audio_profile.TabIndex = 2;
            btn_create_audio_profile.Text = "Create New Profile from Current Audio";
            btn_create_audio_profile.UseVisualStyleBackColor = true;
            btn_create_audio_profile.Click += btn_create_audio_profile_Click;
            // 
            // btn_update_audio_profile
            // 
            btn_update_audio_profile.Enabled = false;
            btn_update_audio_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update_audio_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update_audio_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update_audio_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_update_audio_profile.ForeColor = System.Drawing.Color.White;
            btn_update_audio_profile.Location = new System.Drawing.Point(74, 536);
            btn_update_audio_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_update_audio_profile.Name = "btn_update_audio_profile";
            btn_update_audio_profile.Size = new System.Drawing.Size(296, 31);
            btn_update_audio_profile.TabIndex = 3;
            btn_update_audio_profile.Text = "Update Profile from Current Audio";
            btn_update_audio_profile.UseVisualStyleBackColor = true;
            btn_update_audio_profile.Click += btn_update_audio_profile_Click;
            // 
            // cb_dont_change_audio
            // 
            cb_dont_change_audio.AutoSize = true;
            cb_dont_change_audio.Checked = true;
            cb_dont_change_audio.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_dont_change_audio.ForeColor = System.Drawing.Color.White;
            cb_dont_change_audio.Location = new System.Drawing.Point(26, 20);
            cb_dont_change_audio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_dont_change_audio.Name = "cb_dont_change_audio";
            cb_dont_change_audio.Size = new System.Drawing.Size(226, 24);
            cb_dont_change_audio.TabIndex = 0;
            cb_dont_change_audio.Text = "Don't change audio settings";
            cb_dont_change_audio.UseVisualStyleBackColor = true;
            cb_dont_change_audio.CheckedChanged += cb_dont_change_audio_CheckedChanged;
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
            tabp_before.Size = new System.Drawing.Size(1264, 750);
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
            flp_start_programs.Size = new System.Drawing.Size(1256, 604);
            flp_start_programs.TabIndex = 0;
            // 
            // p_start_program_upper
            // 
            p_start_program_upper.Controls.Add(btn_find_examples_startprograms);
            p_start_program_upper.Controls.Add(btn_add_new_stop_program);
            p_start_program_upper.Controls.Add(btn_add_new_start_program);
            p_start_program_upper.Controls.Add(label3);
            p_start_program_upper.Dock = System.Windows.Forms.DockStyle.Top;
            p_start_program_upper.Location = new System.Drawing.Point(4, 3);
            p_start_program_upper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_start_program_upper.Name = "p_start_program_upper";
            p_start_program_upper.Size = new System.Drawing.Size(1256, 140);
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
            btn_find_examples_startprograms.Location = new System.Drawing.Point(1100, 90);
            btn_find_examples_startprograms.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_find_examples_startprograms.Name = "btn_find_examples_startprograms";
            btn_find_examples_startprograms.Size = new System.Drawing.Size(136, 29);
            btn_find_examples_startprograms.TabIndex = 43;
            btn_find_examples_startprograms.Text = "Show me &Examples";
            btn_find_examples_startprograms.UseVisualStyleBackColor = true;
            btn_find_examples_startprograms.Click += btn_find_examples_startprograms_Click;
            // 
            // btn_add_new_stop_program
            // 
            btn_add_new_stop_program.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btn_add_new_stop_program.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_add_new_stop_program.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_add_new_stop_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_add_new_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_add_new_stop_program.ForeColor = System.Drawing.Color.White;
            btn_add_new_stop_program.Location = new System.Drawing.Point(596, 73);
            btn_add_new_stop_program.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_add_new_stop_program.Name = "btn_add_new_stop_program";
            btn_add_new_stop_program.Size = new System.Drawing.Size(287, 46);
            btn_add_new_stop_program.TabIndex = 44;
            btn_add_new_stop_program.Text = "Add &Stop Program";
            btn_add_new_stop_program.UseVisualStyleBackColor = true;
            btn_add_new_stop_program.Click += btn_add_new_stop_program_Click;
            // 
            // btn_add_new_start_program
            // 
            btn_add_new_start_program.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btn_add_new_start_program.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_add_new_start_program.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_add_new_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_add_new_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_add_new_start_program.ForeColor = System.Drawing.Color.White;
            btn_add_new_start_program.Location = new System.Drawing.Point(291, 73);
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
            label3.Location = new System.Drawing.Point(129, 23);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(904, 20);
            label3.TabIndex = 42;
            label3.Text = "Add one or more additional programs to start or stop before the main Game runs. They will start or stop in the order listed below.";
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
            tabp_game.Size = new System.Drawing.Size(1264, 750);
            tabp_game.TabIndex = 2;
            tabp_game.Text = "4. Choose Game to start";
            // 
            // p_game_list
            // 
            p_game_list.Controls.Add(ilv_games);
            p_game_list.Dock = System.Windows.Forms.DockStyle.Fill;
            p_game_list.Location = new System.Drawing.Point(4, 578);
            p_game_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_game_list.Name = "p_game_list";
            p_game_list.Size = new System.Drawing.Size(1256, 169);
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
            ilv_games.Size = new System.Drawing.Size(1256, 169);
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
            p_gametostart.Size = new System.Drawing.Size(1256, 575);
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
            btn_find_examples_game.Location = new System.Drawing.Point(1114, 13);
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
            p_standalone.Location = new System.Drawing.Point(21, 78);
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
            nud_timeout_executable.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nud_timeout_executable.Name = "nud_timeout_executable";
            nud_timeout_executable.Size = new System.Drawing.Size(64, 26);
            nud_timeout_executable.TabIndex = 6;
            nud_timeout_executable.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // rb_standalone
            // 
            rb_standalone.AutoSize = true;
            rb_standalone.ForeColor = System.Drawing.Color.White;
            rb_standalone.Location = new System.Drawing.Point(18, 47);
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
            p_game.Location = new System.Drawing.Point(22, 355);
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
            lbl_no_game_libraries.Size = new System.Drawing.Size(657, 22);
            lbl_no_game_libraries.TabIndex = 34;
            lbl_no_game_libraries.Text = "No supported game libraries detected! (Steam, Origin, Uplay, Epic, GOG or Xbox supported)";
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
            cb_wait_alternative_game.Location = new System.Drawing.Point(193, 115);
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
            nud_timeout_game.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            nud_timeout_game.Name = "nud_timeout_game";
            nud_timeout_game.Size = new System.Drawing.Size(63, 26);
            nud_timeout_game.TabIndex = 5;
            nud_timeout_game.Value = new decimal(new int[] { 60, 0, 0, 0 });
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
            rb_launcher.Location = new System.Drawing.Point(18, 326);
            rb_launcher.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rb_launcher.Name = "rb_launcher";
            rb_launcher.Size = new System.Drawing.Size(574, 24);
            rb_launcher.TabIndex = 42;
            rb_launcher.TabStop = true;
            rb_launcher.Text = "Launch a Game installed in Steam, Origin, Uplay, Epic, GOG or Xbox Libraries";
            rb_launcher.UseVisualStyleBackColor = true;
            rb_launcher.CheckedChanged += rb_launcher_CheckedChanged;
            rb_launcher.Paint += radiobutton_Paint;
            // 
            // tabp_after
            // 
            tabp_after.BackColor = System.Drawing.Color.Black;
            tabp_after.Controls.Add(groupBox3);
            tabp_after.Controls.Add(groupBox1);
            tabp_after.Controls.Add(gb_display_after);
            tabp_after.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabp_after.ForeColor = System.Drawing.Color.White;
            tabp_after.Location = new System.Drawing.Point(4, 32);
            tabp_after.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_after.Name = "tabp_after";
            tabp_after.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabp_after.Size = new System.Drawing.Size(1264, 750);
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
            groupBox3.Location = new System.Drawing.Point(204, 526);
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
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            groupBox1.Controls.Add(rb_switch_audio_permanent);
            groupBox1.Controls.Add(rb_switch_audio_temp);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox1.ForeColor = System.Drawing.Color.White;
            groupBox1.Location = new System.Drawing.Point(204, 194);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(892, 145);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "What happens to the Audio output afterwards?";
            // 
            // rb_switch_audio_permanent
            // 
            rb_switch_audio_permanent.AutoSize = true;
            rb_switch_audio_permanent.ForeColor = System.Drawing.Color.White;
            rb_switch_audio_permanent.Location = new System.Drawing.Point(114, 86);
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
            rb_switch_audio_temp.Location = new System.Drawing.Point(114, 44);
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
            gb_display_after.Location = new System.Drawing.Point(204, 32);
            gb_display_after.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_display_after.Name = "gb_display_after";
            gb_display_after.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_display_after.Size = new System.Drawing.Size(892, 143);
            gb_display_after.TabIndex = 11;
            gb_display_after.TabStop = false;
            gb_display_after.Text = "What happens to the Display Profile afterwards?";
            // 
            // rb_switch_display_permanent
            // 
            rb_switch_display_permanent.AutoSize = true;
            rb_switch_display_permanent.ForeColor = System.Drawing.Color.White;
            rb_switch_display_permanent.Location = new System.Drawing.Point(114, 84);
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
            rb_switch_display_temp.Location = new System.Drawing.Point(114, 45);
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
            txt_shortcut_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_shortcut_save_name.Location = new System.Drawing.Point(241, 876);
            txt_shortcut_save_name.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_shortcut_save_name.MaxLength = 200;
            txt_shortcut_save_name.Name = "txt_shortcut_save_name";
            txt_shortcut_save_name.Size = new System.Drawing.Size(867, 31);
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
            lbl_shortcut_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_shortcut_name.ForeColor = System.Drawing.Color.Transparent;
            lbl_shortcut_name.Location = new System.Drawing.Point(27, 879);
            lbl_shortcut_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_shortcut_name.Name = "lbl_shortcut_name";
            lbl_shortcut_name.Size = new System.Drawing.Size(160, 25);
            lbl_shortcut_name.TabIndex = 31;
            lbl_shortcut_name.Text = "Shortcut Name:";
            // 
            // cb_autosuggest
            // 
            cb_autosuggest.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cb_autosuggest.Checked = true;
            cb_autosuggest.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_autosuggest.ForeColor = System.Drawing.Color.White;
            cb_autosuggest.Location = new System.Drawing.Point(1148, 882);
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
            btn_hotkey.Location = new System.Drawing.Point(507, 928);
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
            lbl_hotkey_assigned.BackColor = System.Drawing.Color.Brown;
            lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_assigned.Location = new System.Drawing.Point(241, 851);
            lbl_hotkey_assigned.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            lbl_hotkey_assigned.Size = new System.Drawing.Size(113, 20);
            lbl_hotkey_assigned.TabIndex = 37;
            lbl_hotkey_assigned.Text = "Hotkeys: None";
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
            btn_help.Location = new System.Drawing.Point(1198, 16);
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
            ClientSize = new System.Drawing.Size(1300, 979);
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
            MinimumSize = new System.Drawing.Size(800, 600);
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
            gb_audio_profile.ResumeLayout(false);
            gb_audio_profile.PerformLayout();
            gb_audio_overrides.ResumeLayout(false);
            gb_audio_overrides.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_speaker_volume).EndInit();
            ((System.ComponentModel.ISupportInitialize)nud_microphone_volume).EndInit();
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
        private System.Windows.Forms.CheckBox cb_dont_change_audio;
        private System.Windows.Forms.Label lbl_audio_profiles;
        private System.Windows.Forms.ListBox lb_audio_profiles;
        private System.Windows.Forms.Button btn_create_audio_profile;
        private System.Windows.Forms.Button btn_update_audio_profile;
        private System.Windows.Forms.Button btn_delete_audio_profile;
        private System.Windows.Forms.Button btn_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_assigned;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btn_run_cmd_afterwards;
        private System.Windows.Forms.TextBox txt_run_cmd_afterwards;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards;
        private System.Windows.Forms.TextBox txt_run_cmd_afterwards_args;
        private System.Windows.Forms.CheckBox cb_run_cmd_afterwards_args;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_switch_audio_permanent;
        private System.Windows.Forms.RadioButton rb_switch_audio_temp;
        private System.Windows.Forms.GroupBox gb_display_after;
        private System.Windows.Forms.RadioButton rb_switch_display_permanent;
        private System.Windows.Forms.RadioButton rb_switch_display_temp;
        private System.Windows.Forms.TabPage tabp_before;
        private System.Windows.Forms.Panel p_start_program_upper;
        private System.Windows.Forms.Button btn_find_examples_startprograms;
        private System.Windows.Forms.Button btn_add_new_start_program;
        private System.Windows.Forms.Button btn_add_new_stop_program;
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
        private System.Windows.Forms.GroupBox gb_audio_profile;
        private System.Windows.Forms.GroupBox gb_audio_overrides;
        private System.Windows.Forms.CheckBox cb_override_microphone_volume;
        private System.Windows.Forms.CheckBox cb_override_speaker_volume;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_microphone_volume;
        private System.Windows.Forms.NumericUpDown nud_speaker_volume;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_audio_profile_settings;
    }
}