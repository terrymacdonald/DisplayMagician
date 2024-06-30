
namespace DisplayMagician.UIForms
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            btn_back = new System.Windows.Forms.Button();
            gb_general = new System.Windows.Forms.GroupBox();
            cb_show_status_action = new System.Windows.Forms.CheckBox();
            cb_show_minimise_action = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            btn_context_menu_uninstall = new System.Windows.Forms.Button();
            cb_show_splashscreen = new System.Windows.Forms.CheckBox();
            cb_start_on_boot = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            cmb_loglevel = new System.Windows.Forms.ComboBox();
            cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            gb_hotkeys = new System.Windows.Forms.GroupBox();
            lbl_hotkey_shortcut_library = new System.Windows.Forms.Label();
            lbl_hotkey_display_profile = new System.Windows.Forms.Label();
            lbl_hotkey_main_window = new System.Windows.Forms.Label();
            lv_dynamic_hotkeys = new System.Windows.Forms.ListView();
            btn_clear_all_hotkeys = new System.Windows.Forms.Button();
            lbl_hotkey_shortcut_library_description = new System.Windows.Forms.Label();
            lbl_hotkey_display_profile_description = new System.Windows.Forms.Label();
            lbl_hotkey_main_window_description = new System.Windows.Forms.Label();
            btn_hotkey_shortcuts = new System.Windows.Forms.Button();
            btn_hotkey_display_profile = new System.Windows.Forms.Button();
            btn_hotkey_main_window = new System.Windows.Forms.Button();
            gb_upgrades = new System.Windows.Forms.GroupBox();
            cb_upgrade_enabled = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            cb_upgrade_prerelease = new System.Windows.Forms.CheckBox();
            gb_support = new System.Windows.Forms.GroupBox();
            btn_create_support_package = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            btn_context_menu_reinstall = new System.Windows.Forms.Button();
            gb_general.SuspendLayout();
            gb_hotkeys.SuspendLayout();
            gb_upgrades.SuspendLayout();
            gb_support.SuspendLayout();
            SuspendLayout();
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1206, 681);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 9;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // gb_general
            // 
            gb_general.Controls.Add(btn_context_menu_reinstall);
            gb_general.Controls.Add(cb_show_status_action);
            gb_general.Controls.Add(cb_show_minimise_action);
            gb_general.Controls.Add(label4);
            gb_general.Controls.Add(btn_context_menu_uninstall);
            gb_general.Controls.Add(cb_show_splashscreen);
            gb_general.Controls.Add(cb_start_on_boot);
            gb_general.Controls.Add(label1);
            gb_general.Controls.Add(cmb_loglevel);
            gb_general.Controls.Add(cb_minimise_notification_area);
            gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_general.ForeColor = System.Drawing.Color.White;
            gb_general.Location = new System.Drawing.Point(31, 24);
            gb_general.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_general.Name = "gb_general";
            gb_general.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_general.Size = new System.Drawing.Size(612, 451);
            gb_general.TabIndex = 11;
            gb_general.TabStop = false;
            gb_general.Text = "General Settings";
            // 
            // cb_show_status_action
            // 
            cb_show_status_action.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_status_action.AutoSize = true;
            cb_show_status_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_status_action.ForeColor = System.Drawing.Color.White;
            cb_show_status_action.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_status_action.Location = new System.Drawing.Point(33, 212);
            cb_show_status_action.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_status_action.Name = "cb_show_status_action";
            cb_show_status_action.Size = new System.Drawing.Size(365, 20);
            cb_show_status_action.TabIndex = 46;
            cb_show_status_action.Text = "Show status change messages in Windows Action Center";
            cb_show_status_action.UseVisualStyleBackColor = true;
            // 
            // cb_show_minimise_action
            // 
            cb_show_minimise_action.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_minimise_action.AutoSize = true;
            cb_show_minimise_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_minimise_action.ForeColor = System.Drawing.Color.White;
            cb_show_minimise_action.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_minimise_action.Location = new System.Drawing.Point(33, 171);
            cb_show_minimise_action.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_minimise_action.Name = "cb_show_minimise_action";
            cb_show_minimise_action.Size = new System.Drawing.Size(486, 20);
            cb_show_minimise_action.TabIndex = 45;
            cb_show_minimise_action.Text = "Show reminder in Windows Action Center when DisplayMagician  is minimised";
            cb_show_minimise_action.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label4.Location = new System.Drawing.Point(122, 335);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(361, 47);
            label4.TabIndex = 44;
            label4.Text = "Add or remove the Desktop Background Context Menu. ";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_context_menu_uninstall
            // 
            btn_context_menu_uninstall.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_context_menu_uninstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_context_menu_uninstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_context_menu_uninstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_context_menu_uninstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_context_menu_uninstall.ForeColor = System.Drawing.Color.White;
            btn_context_menu_uninstall.Location = new System.Drawing.Point(342, 385);
            btn_context_menu_uninstall.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_context_menu_uninstall.Name = "btn_context_menu_uninstall";
            btn_context_menu_uninstall.Size = new System.Drawing.Size(243, 38);
            btn_context_menu_uninstall.TabIndex = 43;
            btn_context_menu_uninstall.Text = "Remove Desktop Context Menu";
            btn_context_menu_uninstall.UseVisualStyleBackColor = true;
            btn_context_menu_uninstall.Click += btn_context_menu_remove_Click;
            // 
            // cb_show_splashscreen
            // 
            cb_show_splashscreen.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_splashscreen.AutoSize = true;
            cb_show_splashscreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_splashscreen.ForeColor = System.Drawing.Color.White;
            cb_show_splashscreen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_splashscreen.Location = new System.Drawing.Point(33, 130);
            cb_show_splashscreen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_splashscreen.Name = "cb_show_splashscreen";
            cb_show_splashscreen.Size = new System.Drawing.Size(311, 20);
            cb_show_splashscreen.TabIndex = 15;
            cb_show_splashscreen.Text = "Show DisplayMagician splash screen on startup";
            cb_show_splashscreen.UseVisualStyleBackColor = true;
            // 
            // cb_start_on_boot
            // 
            cb_start_on_boot.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_start_on_boot.AutoSize = true;
            cb_start_on_boot.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_start_on_boot.ForeColor = System.Drawing.Color.White;
            cb_start_on_boot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_start_on_boot.Location = new System.Drawing.Point(33, 48);
            cb_start_on_boot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_start_on_boot.Name = "cb_start_on_boot";
            cb_start_on_boot.Size = new System.Drawing.Size(388, 20);
            cb_start_on_boot.TabIndex = 14;
            cb_start_on_boot.Text = "Start DisplayMagician automatically when the computer starts";
            cb_start_on_boot.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.Transparent;
            label1.Location = new System.Drawing.Point(30, 260);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(136, 16);
            label1.TabIndex = 13;
            label1.Text = "What type of logging?";
            // 
            // cmb_loglevel
            // 
            cmb_loglevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_loglevel.FormattingEnabled = true;
            cmb_loglevel.Location = new System.Drawing.Point(197, 256);
            cmb_loglevel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_loglevel.Name = "cmb_loglevel";
            cmb_loglevel.Size = new System.Drawing.Size(388, 24);
            cmb_loglevel.TabIndex = 12;
            // 
            // cb_minimise_notification_area
            // 
            cb_minimise_notification_area.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_minimise_notification_area.AutoSize = true;
            cb_minimise_notification_area.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            cb_minimise_notification_area.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_minimise_notification_area.Location = new System.Drawing.Point(33, 90);
            cb_minimise_notification_area.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            cb_minimise_notification_area.Size = new System.Drawing.Size(331, 20);
            cb_minimise_notification_area.TabIndex = 11;
            cb_minimise_notification_area.Text = "Start DisplayMagician minimised in notification area";
            cb_minimise_notification_area.UseVisualStyleBackColor = true;
            // 
            // gb_hotkeys
            // 
            gb_hotkeys.Controls.Add(lbl_hotkey_shortcut_library);
            gb_hotkeys.Controls.Add(lbl_hotkey_display_profile);
            gb_hotkeys.Controls.Add(lbl_hotkey_main_window);
            gb_hotkeys.Controls.Add(lv_dynamic_hotkeys);
            gb_hotkeys.Controls.Add(btn_clear_all_hotkeys);
            gb_hotkeys.Controls.Add(lbl_hotkey_shortcut_library_description);
            gb_hotkeys.Controls.Add(lbl_hotkey_display_profile_description);
            gb_hotkeys.Controls.Add(lbl_hotkey_main_window_description);
            gb_hotkeys.Controls.Add(btn_hotkey_shortcuts);
            gb_hotkeys.Controls.Add(btn_hotkey_display_profile);
            gb_hotkeys.Controls.Add(btn_hotkey_main_window);
            gb_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_hotkeys.ForeColor = System.Drawing.Color.White;
            gb_hotkeys.Location = new System.Drawing.Point(681, 24);
            gb_hotkeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_hotkeys.Name = "gb_hotkeys";
            gb_hotkeys.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_hotkeys.Size = new System.Drawing.Size(612, 451);
            gb_hotkeys.TabIndex = 12;
            gb_hotkeys.TabStop = false;
            gb_hotkeys.Text = "Hotkeys";
            // 
            // lbl_hotkey_shortcut_library
            // 
            lbl_hotkey_shortcut_library.Location = new System.Drawing.Point(275, 153);
            lbl_hotkey_shortcut_library.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_shortcut_library.Name = "lbl_hotkey_shortcut_library";
            lbl_hotkey_shortcut_library.Size = new System.Drawing.Size(197, 18);
            lbl_hotkey_shortcut_library.TabIndex = 47;
            lbl_hotkey_shortcut_library.Text = "None Set";
            lbl_hotkey_shortcut_library.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lbl_hotkey_shortcut_library.Click += lbl_hotkey_shortcut_library_Click;
            // 
            // lbl_hotkey_display_profile
            // 
            lbl_hotkey_display_profile.Location = new System.Drawing.Point(275, 96);
            lbl_hotkey_display_profile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_display_profile.Name = "lbl_hotkey_display_profile";
            lbl_hotkey_display_profile.Size = new System.Drawing.Size(197, 18);
            lbl_hotkey_display_profile.TabIndex = 46;
            lbl_hotkey_display_profile.Text = "None Set";
            lbl_hotkey_display_profile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lbl_hotkey_display_profile.Click += lbl_hotkey_display_profile_Click;
            // 
            // lbl_hotkey_main_window
            // 
            lbl_hotkey_main_window.Location = new System.Drawing.Point(276, 40);
            lbl_hotkey_main_window.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_main_window.Name = "lbl_hotkey_main_window";
            lbl_hotkey_main_window.Size = new System.Drawing.Size(197, 18);
            lbl_hotkey_main_window.TabIndex = 45;
            lbl_hotkey_main_window.Text = "None Set";
            lbl_hotkey_main_window.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lbl_hotkey_main_window.Click += lbl_hotkey_main_window_Click;
            // 
            // lv_dynamic_hotkeys
            // 
            lv_dynamic_hotkeys.Location = new System.Drawing.Point(31, 204);
            lv_dynamic_hotkeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lv_dynamic_hotkeys.Name = "lv_dynamic_hotkeys";
            lv_dynamic_hotkeys.Size = new System.Drawing.Size(551, 164);
            lv_dynamic_hotkeys.TabIndex = 44;
            lv_dynamic_hotkeys.UseCompatibleStateImageBehavior = false;
            // 
            // btn_clear_all_hotkeys
            // 
            btn_clear_all_hotkeys.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_clear_all_hotkeys.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear_all_hotkeys.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear_all_hotkeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear_all_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_clear_all_hotkeys.ForeColor = System.Drawing.Color.White;
            btn_clear_all_hotkeys.Location = new System.Drawing.Point(230, 385);
            btn_clear_all_hotkeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear_all_hotkeys.Name = "btn_clear_all_hotkeys";
            btn_clear_all_hotkeys.Size = new System.Drawing.Size(148, 38);
            btn_clear_all_hotkeys.TabIndex = 42;
            btn_clear_all_hotkeys.Text = "Clear All Hotkeys";
            btn_clear_all_hotkeys.UseVisualStyleBackColor = true;
            btn_clear_all_hotkeys.Click += btn_clear_all_hotkeys_Click;
            // 
            // lbl_hotkey_shortcut_library_description
            // 
            lbl_hotkey_shortcut_library_description.AutoSize = true;
            lbl_hotkey_shortcut_library_description.Location = new System.Drawing.Point(29, 153);
            lbl_hotkey_shortcut_library_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_shortcut_library_description.Name = "lbl_hotkey_shortcut_library_description";
            lbl_hotkey_shortcut_library_description.Size = new System.Drawing.Size(196, 16);
            lbl_hotkey_shortcut_library_description.TabIndex = 41;
            lbl_hotkey_shortcut_library_description.Text = "Hotkey to open Shortcut Library:";
            // 
            // lbl_hotkey_display_profile_description
            // 
            lbl_hotkey_display_profile_description.AutoSize = true;
            lbl_hotkey_display_profile_description.Location = new System.Drawing.Point(29, 96);
            lbl_hotkey_display_profile_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_display_profile_description.Name = "lbl_hotkey_display_profile_description";
            lbl_hotkey_display_profile_description.Size = new System.Drawing.Size(242, 16);
            lbl_hotkey_display_profile_description.TabIndex = 40;
            lbl_hotkey_display_profile_description.Text = "Hotkey to open Display Profile Window:";
            // 
            // lbl_hotkey_main_window_description
            // 
            lbl_hotkey_main_window_description.AutoSize = true;
            lbl_hotkey_main_window_description.Location = new System.Drawing.Point(30, 40);
            lbl_hotkey_main_window_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_main_window_description.Name = "lbl_hotkey_main_window_description";
            lbl_hotkey_main_window_description.Size = new System.Drawing.Size(184, 16);
            lbl_hotkey_main_window_description.TabIndex = 39;
            lbl_hotkey_main_window_description.Text = "Hotkey to open Main Window:";
            // 
            // btn_hotkey_shortcuts
            // 
            btn_hotkey_shortcuts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_hotkey_shortcuts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_shortcuts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_shortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_shortcuts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_shortcuts.ForeColor = System.Drawing.Color.White;
            btn_hotkey_shortcuts.Location = new System.Drawing.Point(479, 144);
            btn_hotkey_shortcuts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_shortcuts.Name = "btn_hotkey_shortcuts";
            btn_hotkey_shortcuts.Size = new System.Drawing.Size(103, 38);
            btn_hotkey_shortcuts.TabIndex = 38;
            btn_hotkey_shortcuts.Text = "Set Hotkey";
            btn_hotkey_shortcuts.UseVisualStyleBackColor = true;
            btn_hotkey_shortcuts.Click += btn_hotkey_shortcuts_Click;
            // 
            // btn_hotkey_display_profile
            // 
            btn_hotkey_display_profile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_hotkey_display_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_display_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_display_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_display_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_display_profile.ForeColor = System.Drawing.Color.White;
            btn_hotkey_display_profile.Location = new System.Drawing.Point(479, 87);
            btn_hotkey_display_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_display_profile.Name = "btn_hotkey_display_profile";
            btn_hotkey_display_profile.Size = new System.Drawing.Size(104, 38);
            btn_hotkey_display_profile.TabIndex = 37;
            btn_hotkey_display_profile.Text = "Set Hotkey";
            btn_hotkey_display_profile.UseVisualStyleBackColor = true;
            btn_hotkey_display_profile.Click += btn_hotkey_display_profile_Click;
            // 
            // btn_hotkey_main_window
            // 
            btn_hotkey_main_window.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_hotkey_main_window.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_main_window.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_main_window.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_main_window.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_main_window.ForeColor = System.Drawing.Color.White;
            btn_hotkey_main_window.Location = new System.Drawing.Point(481, 31);
            btn_hotkey_main_window.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_main_window.Name = "btn_hotkey_main_window";
            btn_hotkey_main_window.Size = new System.Drawing.Size(103, 38);
            btn_hotkey_main_window.TabIndex = 36;
            btn_hotkey_main_window.Text = "Set Hotkey";
            btn_hotkey_main_window.UseVisualStyleBackColor = true;
            btn_hotkey_main_window.Click += btn_hotkey_main_window_Click;
            // 
            // gb_upgrades
            // 
            gb_upgrades.Controls.Add(cb_upgrade_enabled);
            gb_upgrades.Controls.Add(label2);
            gb_upgrades.Controls.Add(cb_upgrade_prerelease);
            gb_upgrades.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_upgrades.ForeColor = System.Drawing.Color.White;
            gb_upgrades.Location = new System.Drawing.Point(31, 509);
            gb_upgrades.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_upgrades.Name = "gb_upgrades";
            gb_upgrades.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_upgrades.Size = new System.Drawing.Size(612, 198);
            gb_upgrades.TabIndex = 13;
            gb_upgrades.TabStop = false;
            gb_upgrades.Text = "Upgrade Settings";
            // 
            // cb_upgrade_enabled
            // 
            cb_upgrade_enabled.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_upgrade_enabled.AutoSize = true;
            cb_upgrade_enabled.Checked = true;
            cb_upgrade_enabled.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_upgrade_enabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_upgrade_enabled.ForeColor = System.Drawing.Color.White;
            cb_upgrade_enabled.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_upgrade_enabled.Location = new System.Drawing.Point(33, 46);
            cb_upgrade_enabled.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_upgrade_enabled.Name = "cb_upgrade_enabled";
            cb_upgrade_enabled.Size = new System.Drawing.Size(399, 20);
            cb_upgrade_enabled.TabIndex = 16;
            cb_upgrade_enabled.Text = "Check if new DisplayMagician updates are available on startup";
            cb_upgrade_enabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(50, 114);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(410, 13);
            label2.TabIndex = 15;
            label2.Text = "(NOTE: beta versions may crash and not work correctly. Use this option with caution!)";
            // 
            // cb_upgrade_prerelease
            // 
            cb_upgrade_prerelease.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_upgrade_prerelease.AutoSize = true;
            cb_upgrade_prerelease.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_upgrade_prerelease.ForeColor = System.Drawing.Color.White;
            cb_upgrade_prerelease.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_upgrade_prerelease.Location = new System.Drawing.Point(33, 89);
            cb_upgrade_prerelease.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_upgrade_prerelease.Name = "cb_upgrade_prerelease";
            cb_upgrade_prerelease.Size = new System.Drawing.Size(410, 20);
            cb_upgrade_prerelease.TabIndex = 14;
            cb_upgrade_prerelease.Text = "Upgrade DisplayMagician to latest beta versions when available";
            cb_upgrade_prerelease.UseVisualStyleBackColor = true;
            // 
            // gb_support
            // 
            gb_support.Controls.Add(btn_create_support_package);
            gb_support.Controls.Add(label3);
            gb_support.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_support.ForeColor = System.Drawing.Color.White;
            gb_support.Location = new System.Drawing.Point(681, 509);
            gb_support.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_support.Name = "gb_support";
            gb_support.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_support.Size = new System.Drawing.Size(612, 137);
            gb_support.TabIndex = 16;
            gb_support.TabStop = false;
            gb_support.Text = "Support Settings";
            // 
            // btn_create_support_package
            // 
            btn_create_support_package.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_create_support_package.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_support_package.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_support_package.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_support_package.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_support_package.ForeColor = System.Drawing.Color.White;
            btn_create_support_package.Location = new System.Drawing.Point(197, 31);
            btn_create_support_package.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_create_support_package.Name = "btn_create_support_package";
            btn_create_support_package.Size = new System.Drawing.Size(214, 38);
            btn_create_support_package.TabIndex = 48;
            btn_create_support_package.Text = "Create a Support Zip File";
            btn_create_support_package.UseVisualStyleBackColor = true;
            btn_create_support_package.Click += btn_create_support_package_Click;
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label3.Location = new System.Drawing.Point(86, 76);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(441, 37);
            label3.TabIndex = 15;
            label3.Text = "Use this button to save a support zip file to your computer. You can then upload this file to GitHub when you have a problem you need me to fix";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_context_menu_reinstall
            // 
            btn_context_menu_reinstall.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_context_menu_reinstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_context_menu_reinstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_context_menu_reinstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_context_menu_reinstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_context_menu_reinstall.ForeColor = System.Drawing.Color.White;
            btn_context_menu_reinstall.Location = new System.Drawing.Point(30, 385);
            btn_context_menu_reinstall.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_context_menu_reinstall.Name = "btn_context_menu_reinstall";
            btn_context_menu_reinstall.Size = new System.Drawing.Size(243, 38);
            btn_context_menu_reinstall.TabIndex = 47;
            btn_context_menu_reinstall.Text = "Add Desktop Context Menu";
            btn_context_menu_reinstall.UseVisualStyleBackColor = true;
            btn_context_menu_reinstall.Click += btn_context_menu_add_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1332, 735);
            Controls.Add(gb_support);
            Controls.Add(gb_upgrades);
            Controls.Add(gb_hotkeys);
            Controls.Add(gb_general);
            Controls.Add(btn_back);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowIcon = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Settings";
            TopMost = true;
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            gb_general.ResumeLayout(false);
            gb_general.PerformLayout();
            gb_hotkeys.ResumeLayout(false);
            gb_hotkeys.PerformLayout();
            gb_upgrades.ResumeLayout(false);
            gb_upgrades.PerformLayout();
            gb_support.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.CheckBox cb_start_on_boot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_loglevel;
        private System.Windows.Forms.CheckBox cb_minimise_notification_area;
        private System.Windows.Forms.GroupBox gb_hotkeys;
        private System.Windows.Forms.Button btn_hotkey_shortcuts;
        private System.Windows.Forms.Button btn_hotkey_display_profile;
        private System.Windows.Forms.Button btn_hotkey_main_window;
        private System.Windows.Forms.Label lbl_hotkey_shortcut_library_description;
        private System.Windows.Forms.Label lbl_hotkey_display_profile_description;
        private System.Windows.Forms.Label lbl_hotkey_main_window_description;
        private System.Windows.Forms.Button btn_clear_all_hotkeys;
        private System.Windows.Forms.ListView lv_dynamic_hotkeys;
        private System.Windows.Forms.Label lbl_hotkey_shortcut_library;
        private System.Windows.Forms.Label lbl_hotkey_display_profile;
        private System.Windows.Forms.Label lbl_hotkey_main_window;
        private System.Windows.Forms.GroupBox gb_upgrades;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_upgrade_prerelease;
        private System.Windows.Forms.GroupBox gb_support;
        private System.Windows.Forms.Button btn_create_support_package;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cb_show_splashscreen;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_context_menu_uninstall;
        private System.Windows.Forms.CheckBox cb_show_status_action;
        private System.Windows.Forms.CheckBox cb_show_minimise_action;
        private System.Windows.Forms.CheckBox cb_upgrade_enabled;
        private System.Windows.Forms.Button btn_context_menu_reinstall;
    }
}