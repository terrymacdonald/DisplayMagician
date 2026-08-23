
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
            label7 = new System.Windows.Forms.Label();
            nud_audio_device_wait = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            cmb_notify_icon_double_click = new System.Windows.Forms.ComboBox();
            cb_wake_up_gpus = new System.Windows.Forms.CheckBox();
            btn_context_menu_reinstall = new System.Windows.Forms.Button();
            cb_show_status_action = new System.Windows.Forms.CheckBox();
            cb_show_message_toasts = new System.Windows.Forms.CheckBox();
            cb_show_minimise_action = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            btn_context_menu_uninstall = new System.Windows.Forms.Button();
            cb_show_splashscreen = new System.Windows.Forms.CheckBox();
            cb_start_on_boot = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            cmb_loglevel = new System.Windows.Forms.ComboBox();
            cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            gb_hotkeys = new System.Windows.Forms.GroupBox();
            lbl_hotkey_exit = new System.Windows.Forms.Label();
            btn_hotkey_exit = new System.Windows.Forms.Button();
            lbl_hotkey_exit_app = new System.Windows.Forms.Label();
            lv_hotkeys = new System.Windows.Forms.ListView();
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
            gb_general.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_audio_device_wait).BeginInit();
            gb_hotkeys.SuspendLayout();
            gb_upgrades.SuspendLayout();
            gb_support.SuspendLayout();
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
            btn_back.Location = new System.Drawing.Point(1396, 709);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 22;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // gb_general
            // 
            gb_general.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_general.Controls.Add(label7);
            gb_general.Controls.Add(nud_audio_device_wait);
            gb_general.Controls.Add(label5);
            gb_general.Controls.Add(label6);
            gb_general.Controls.Add(cmb_notify_icon_double_click);
            gb_general.Controls.Add(cb_wake_up_gpus);
            gb_general.Controls.Add(btn_context_menu_reinstall);
            gb_general.Controls.Add(cb_show_status_action);
            gb_general.Controls.Add(cb_show_message_toasts);
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
            gb_general.Size = new System.Drawing.Size(612, 503);
            gb_general.TabIndex = 0;
            gb_general.TabStop = false;
            gb_general.Text = "General Settings";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label7.ForeColor = System.Drawing.Color.Transparent;
            label7.Location = new System.Drawing.Point(492, 266);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(59, 16);
            label7.TabIndex = 0;
            label7.Text = "seconds";
            // 
            // nud_audio_device_wait
            // 
            nud_audio_device_wait.Location = new System.Drawing.Point(429, 264);
            nud_audio_device_wait.Maximum = new decimal(new int[] { 45, 0, 0, 0 });
            nud_audio_device_wait.Name = "nud_audio_device_wait";
            nud_audio_device_wait.Size = new System.Drawing.Size(60, 22);
            nud_audio_device_wait.TabIndex = 8;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label5.ForeColor = System.Drawing.Color.Transparent;
            label5.Location = new System.Drawing.Point(33, 266);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(386, 16);
            label5.TabIndex = 0;
            label5.Text = "Max time to wait for audio device to appear (default 10 seconds):";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label6.ForeColor = System.Drawing.Color.Transparent;
            label6.Location = new System.Drawing.Point(32, 303);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(308, 16);
            label6.TabIndex = 0;
            label6.Text = "Notification Icon (System Tray) double click action :";
            // 
            // cmb_notify_icon_double_click
            // 
            cmb_notify_icon_double_click.FormattingEnabled = true;
            cmb_notify_icon_double_click.ItemHeight = 16;
            cmb_notify_icon_double_click.Location = new System.Drawing.Point(359, 300);
            cmb_notify_icon_double_click.Name = "cmb_notify_icon_double_click";
            cmb_notify_icon_double_click.Size = new System.Drawing.Size(226, 24);
            cmb_notify_icon_double_click.TabIndex = 49;
            // 
            // cb_wake_up_gpus
            // 
            cb_wake_up_gpus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_wake_up_gpus.AutoSize = true;
            cb_wake_up_gpus.Checked = true;
            cb_wake_up_gpus.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_wake_up_gpus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_wake_up_gpus.ForeColor = System.Drawing.Color.White;
            cb_wake_up_gpus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_wake_up_gpus.Location = new System.Drawing.Point(32, 232);
            cb_wake_up_gpus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_wake_up_gpus.Name = "cb_wake_up_gpus";
            cb_wake_up_gpus.Size = new System.Drawing.Size(389, 20);
            cb_wake_up_gpus.TabIndex = 7;
            cb_wake_up_gpus.Text = "Keep GPUs awake to make laptops display changes reliable";
            cb_wake_up_gpus.UseVisualStyleBackColor = true;
            // 
            // btn_context_menu_reinstall
            // 
            btn_context_menu_reinstall.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_context_menu_reinstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_context_menu_reinstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_context_menu_reinstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_context_menu_reinstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_context_menu_reinstall.ForeColor = System.Drawing.Color.White;
            btn_context_menu_reinstall.Location = new System.Drawing.Point(30, 437);
            btn_context_menu_reinstall.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_context_menu_reinstall.Name = "btn_context_menu_reinstall";
            btn_context_menu_reinstall.Size = new System.Drawing.Size(243, 38);
            btn_context_menu_reinstall.TabIndex = 11;
            btn_context_menu_reinstall.Text = "Add Desktop Context Menu";
            btn_context_menu_reinstall.UseVisualStyleBackColor = true;
            btn_context_menu_reinstall.Click += btn_context_menu_add_Click;
            // 
            // cb_show_status_action
            // 
            cb_show_status_action.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_status_action.AutoSize = true;
            cb_show_status_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_status_action.ForeColor = System.Drawing.Color.White;
            cb_show_status_action.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_status_action.Location = new System.Drawing.Point(33, 169);
            cb_show_status_action.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_status_action.Name = "cb_show_status_action";
            cb_show_status_action.Size = new System.Drawing.Size(365, 20);
            cb_show_status_action.TabIndex = 5;
            cb_show_status_action.Text = "Show status change messages in Windows Action Center";
            cb_show_status_action.UseVisualStyleBackColor = true;
            // 
            // cb_show_message_toasts
            // 
            cb_show_message_toasts.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_message_toasts.AutoSize = true;
            cb_show_message_toasts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_message_toasts.ForeColor = System.Drawing.Color.White;
            cb_show_message_toasts.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_message_toasts.Location = new System.Drawing.Point(32, 201);
            cb_show_message_toasts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_message_toasts.Name = "cb_show_message_toasts";
            cb_show_message_toasts.Size = new System.Drawing.Size(372, 20);
            cb_show_message_toasts.TabIndex = 6;
            cb_show_message_toasts.Text = "Show new message notifications in Windows Action Center";
            cb_show_message_toasts.UseVisualStyleBackColor = true;
            // 
            // cb_show_minimise_action
            // 
            cb_show_minimise_action.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_show_minimise_action.AutoSize = true;
            cb_show_minimise_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_minimise_action.ForeColor = System.Drawing.Color.White;
            cb_show_minimise_action.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_show_minimise_action.Location = new System.Drawing.Point(33, 136);
            cb_show_minimise_action.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_minimise_action.Name = "cb_show_minimise_action";
            cb_show_minimise_action.Size = new System.Drawing.Size(486, 20);
            cb_show_minimise_action.TabIndex = 4;
            cb_show_minimise_action.Text = "Show reminder in Windows Action Center when DisplayMagician  is minimised";
            cb_show_minimise_action.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label4.Location = new System.Drawing.Point(130, 399);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(361, 28);
            label4.TabIndex = 0;
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
            btn_context_menu_uninstall.Location = new System.Drawing.Point(342, 437);
            btn_context_menu_uninstall.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_context_menu_uninstall.Name = "btn_context_menu_uninstall";
            btn_context_menu_uninstall.Size = new System.Drawing.Size(243, 38);
            btn_context_menu_uninstall.TabIndex = 12;
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
            cb_show_splashscreen.Location = new System.Drawing.Point(33, 104);
            cb_show_splashscreen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_show_splashscreen.Name = "cb_show_splashscreen";
            cb_show_splashscreen.Size = new System.Drawing.Size(311, 20);
            cb_show_splashscreen.TabIndex = 3;
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
            cb_start_on_boot.Location = new System.Drawing.Point(33, 39);
            cb_start_on_boot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_start_on_boot.Name = "cb_start_on_boot";
            cb_start_on_boot.Size = new System.Drawing.Size(388, 20);
            cb_start_on_boot.TabIndex = 1;
            cb_start_on_boot.Text = "Start DisplayMagician automatically when the computer starts";
            cb_start_on_boot.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.Transparent;
            label1.Location = new System.Drawing.Point(32, 353);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(136, 16);
            label1.TabIndex = 0;
            label1.Text = "What type of logging?";
            // 
            // cmb_loglevel
            // 
            cmb_loglevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_loglevel.FormattingEnabled = true;
            cmb_loglevel.ItemHeight = 16;
            cmb_loglevel.Location = new System.Drawing.Point(179, 350);
            cmb_loglevel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmb_loglevel.Name = "cmb_loglevel";
            cmb_loglevel.Size = new System.Drawing.Size(406, 24);
            cmb_loglevel.TabIndex = 12;
            // 
            // cb_minimise_notification_area
            // 
            cb_minimise_notification_area.Anchor = System.Windows.Forms.AnchorStyles.Top;
            cb_minimise_notification_area.AutoSize = true;
            cb_minimise_notification_area.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            cb_minimise_notification_area.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            cb_minimise_notification_area.Location = new System.Drawing.Point(33, 71);
            cb_minimise_notification_area.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            cb_minimise_notification_area.Size = new System.Drawing.Size(331, 20);
            cb_minimise_notification_area.TabIndex = 2;
            cb_minimise_notification_area.Text = "Start DisplayMagician minimised in notification area";
            cb_minimise_notification_area.UseVisualStyleBackColor = true;
            // 
            // gb_hotkeys
            // 
            gb_hotkeys.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_hotkeys.Controls.Add(lbl_hotkey_exit);
            gb_hotkeys.Controls.Add(btn_hotkey_exit);
            gb_hotkeys.Controls.Add(lbl_hotkey_exit_app);
            gb_hotkeys.Controls.Add(lv_hotkeys);
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
            gb_hotkeys.Size = new System.Drawing.Size(803, 503);
            gb_hotkeys.TabIndex = 0;
            gb_hotkeys.TabStop = false;
            gb_hotkeys.Text = "Hotkeys";
            // 
            // lbl_hotkey_exit
            // 
            lbl_hotkey_exit.AutoSize = true;
            lbl_hotkey_exit.Location = new System.Drawing.Point(456, 40);
            lbl_hotkey_exit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_exit.Name = "lbl_hotkey_exit";
            lbl_hotkey_exit.Size = new System.Drawing.Size(194, 16);
            lbl_hotkey_exit.TabIndex = 0;
            lbl_hotkey_exit.Text = "Hotkey to exit DisplayMagician:";
            lbl_hotkey_exit.Click += lbl_hotkey_exit_Click;
            // 
            // btn_hotkey_exit
            // 
            btn_hotkey_exit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_hotkey_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_exit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_exit.ForeColor = System.Drawing.Color.White;
            btn_hotkey_exit.Location = new System.Drawing.Point(658, 35);
            btn_hotkey_exit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_exit.Name = "btn_hotkey_exit";
            btn_hotkey_exit.Size = new System.Drawing.Size(103, 27);
            btn_hotkey_exit.TabIndex = 17;
            btn_hotkey_exit.Text = "Set Hotkey";
            btn_hotkey_exit.UseVisualStyleBackColor = true;
            btn_hotkey_exit.Click += btn_hotkey_exit_app_Click;
            // 
            // lbl_hotkey_exit_app
            // 
            lbl_hotkey_exit_app.AutoSize = true;
            lbl_hotkey_exit_app.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_exit_app.Location = new System.Drawing.Point(329, 148);
            lbl_hotkey_exit_app.Name = "lbl_hotkey_exit_app";
            lbl_hotkey_exit_app.Size = new System.Drawing.Size(137, 20);
            lbl_hotkey_exit_app.TabIndex = 0;
            lbl_hotkey_exit_app.Text = "All Saved Hotkeys";
            lbl_hotkey_exit_app.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lv_hotkeys
            // 
            lv_hotkeys.Location = new System.Drawing.Point(37, 171);
            lv_hotkeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lv_hotkeys.Name = "lv_hotkeys";
            lv_hotkeys.ShowGroups = false;
            lv_hotkeys.Size = new System.Drawing.Size(724, 238);
            lv_hotkeys.TabIndex = 19;
            lv_hotkeys.UseCompatibleStateImageBehavior = false;
            lv_hotkeys.View = System.Windows.Forms.View.Details;
            lv_hotkeys.MouseClick += lv_hotkeys_MouseClick;
            // 
            // btn_clear_all_hotkeys
            // 
            btn_clear_all_hotkeys.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_clear_all_hotkeys.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear_all_hotkeys.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear_all_hotkeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear_all_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_clear_all_hotkeys.ForeColor = System.Drawing.Color.White;
            btn_clear_all_hotkeys.Location = new System.Drawing.Point(321, 437);
            btn_clear_all_hotkeys.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear_all_hotkeys.Name = "btn_clear_all_hotkeys";
            btn_clear_all_hotkeys.Size = new System.Drawing.Size(148, 38);
            btn_clear_all_hotkeys.TabIndex = 20;
            btn_clear_all_hotkeys.Text = "Clear All Hotkeys";
            btn_clear_all_hotkeys.UseVisualStyleBackColor = true;
            btn_clear_all_hotkeys.Click += btn_clear_all_hotkeys_Click;
            // 
            // lbl_hotkey_shortcut_library_description
            // 
            lbl_hotkey_shortcut_library_description.AutoSize = true;
            lbl_hotkey_shortcut_library_description.Location = new System.Drawing.Point(454, 85);
            lbl_hotkey_shortcut_library_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_shortcut_library_description.Name = "lbl_hotkey_shortcut_library_description";
            lbl_hotkey_shortcut_library_description.Size = new System.Drawing.Size(196, 16);
            lbl_hotkey_shortcut_library_description.TabIndex = 0;
            lbl_hotkey_shortcut_library_description.Text = "Hotkey to open Shortcut Library:";
            // 
            // lbl_hotkey_display_profile_description
            // 
            lbl_hotkey_display_profile_description.AutoSize = true;
            lbl_hotkey_display_profile_description.Location = new System.Drawing.Point(47, 85);
            lbl_hotkey_display_profile_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_display_profile_description.Name = "lbl_hotkey_display_profile_description";
            lbl_hotkey_display_profile_description.Size = new System.Drawing.Size(242, 16);
            lbl_hotkey_display_profile_description.TabIndex = 0;
            lbl_hotkey_display_profile_description.Text = "Hotkey to open Display Profile Window:";
            // 
            // lbl_hotkey_main_window_description
            // 
            lbl_hotkey_main_window_description.AutoSize = true;
            lbl_hotkey_main_window_description.Location = new System.Drawing.Point(105, 40);
            lbl_hotkey_main_window_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_main_window_description.Name = "lbl_hotkey_main_window_description";
            lbl_hotkey_main_window_description.Size = new System.Drawing.Size(184, 16);
            lbl_hotkey_main_window_description.TabIndex = 0;
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
            btn_hotkey_shortcuts.Location = new System.Drawing.Point(658, 80);
            btn_hotkey_shortcuts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_shortcuts.Name = "btn_hotkey_shortcuts";
            btn_hotkey_shortcuts.Size = new System.Drawing.Size(103, 27);
            btn_hotkey_shortcuts.TabIndex = 18;
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
            btn_hotkey_display_profile.Location = new System.Drawing.Point(297, 80);
            btn_hotkey_display_profile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_display_profile.Name = "btn_hotkey_display_profile";
            btn_hotkey_display_profile.Size = new System.Drawing.Size(104, 27);
            btn_hotkey_display_profile.TabIndex = 16;
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
            btn_hotkey_main_window.Location = new System.Drawing.Point(297, 35);
            btn_hotkey_main_window.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey_main_window.Name = "btn_hotkey_main_window";
            btn_hotkey_main_window.Size = new System.Drawing.Size(103, 27);
            btn_hotkey_main_window.TabIndex = 15;
            btn_hotkey_main_window.Text = "Set Hotkey";
            btn_hotkey_main_window.UseVisualStyleBackColor = true;
            btn_hotkey_main_window.Click += btn_hotkey_main_window_Click;
            // 
            // gb_upgrades
            // 
            gb_upgrades.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_upgrades.Controls.Add(cb_upgrade_enabled);
            gb_upgrades.Controls.Add(label2);
            gb_upgrades.Controls.Add(cb_upgrade_prerelease);
            gb_upgrades.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_upgrades.ForeColor = System.Drawing.Color.White;
            gb_upgrades.Location = new System.Drawing.Point(31, 555);
            gb_upgrades.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_upgrades.Name = "gb_upgrades";
            gb_upgrades.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_upgrades.Size = new System.Drawing.Size(612, 177);
            gb_upgrades.TabIndex = 0;
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
            cb_upgrade_enabled.TabIndex = 13;
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
            gb_support.Anchor = System.Windows.Forms.AnchorStyles.None;
            gb_support.Controls.Add(btn_create_support_package);
            gb_support.Controls.Add(label3);
            gb_support.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_support.ForeColor = System.Drawing.Color.White;
            gb_support.Location = new System.Drawing.Point(681, 555);
            gb_support.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_support.Name = "gb_support";
            gb_support.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_support.Size = new System.Drawing.Size(803, 116);
            gb_support.TabIndex = 0;
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
            btn_create_support_package.Location = new System.Drawing.Point(292, 26);
            btn_create_support_package.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_create_support_package.Name = "btn_create_support_package";
            btn_create_support_package.Size = new System.Drawing.Size(214, 38);
            btn_create_support_package.TabIndex = 21;
            btn_create_support_package.Text = "Create a Support Zip File";
            btn_create_support_package.UseVisualStyleBackColor = true;
            btn_create_support_package.Click += btn_create_support_package_Click;
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label3.Location = new System.Drawing.Point(180, 66);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(441, 37);
            label3.TabIndex = 0;
            label3.Text = "Use this button to save a support zip file to your computer. You can then upload this file to GitHub when you have a problem you need me to fix";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1528, 767);
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
            MinimumSize = new System.Drawing.Size(1544, 806);
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
            ((System.ComponentModel.ISupportInitialize)nud_audio_device_wait).EndInit();
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
        private System.Windows.Forms.CheckBox cb_show_message_toasts;
        private System.Windows.Forms.CheckBox cb_show_minimise_action;
        private System.Windows.Forms.CheckBox cb_upgrade_enabled;
        private System.Windows.Forms.Button btn_context_menu_reinstall;
        private System.Windows.Forms.ListView lv_hotkeys;
        private System.Windows.Forms.Label lbl_hotkey_exit_app;
        private System.Windows.Forms.Label lbl_hotkey_exit;
        private System.Windows.Forms.Button btn_hotkey_exit;
        private System.Windows.Forms.CheckBox cb_wake_up_gpus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmb_notify_icon_double_click;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nud_audio_device_wait;
    }
}