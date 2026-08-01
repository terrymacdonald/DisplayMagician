
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
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            gb_general = new System.Windows.Forms.GroupBox();
            tlp_general = new System.Windows.Forms.TableLayoutPanel();
            cb_start_on_boot = new System.Windows.Forms.CheckBox();
            cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            cb_show_splashscreen = new System.Windows.Forms.CheckBox();
            cb_show_minimise_action = new System.Windows.Forms.CheckBox();
            cb_show_status_action = new System.Windows.Forms.CheckBox();
            cb_show_message_toasts = new System.Windows.Forms.CheckBox();
            cb_wake_up_gpus = new System.Windows.Forms.CheckBox();
            tlp_audio_wait = new System.Windows.Forms.TableLayoutPanel();
            label5 = new System.Windows.Forms.Label();
            nud_audio_device_wait = new System.Windows.Forms.NumericUpDown();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            cmb_notify_icon_double_click = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            cmb_loglevel = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            tlp_context_menu = new System.Windows.Forms.TableLayoutPanel();
            btn_context_menu_reinstall = new System.Windows.Forms.Button();
            btn_context_menu_uninstall = new System.Windows.Forms.Button();
            gb_hotkeys = new System.Windows.Forms.GroupBox();
            tlp_hotkeys = new System.Windows.Forms.TableLayoutPanel();
            lbl_hotkey_main_window_description = new System.Windows.Forms.Label();
            btn_hotkey_main_window = new System.Windows.Forms.Button();
            lbl_hotkey_exit = new System.Windows.Forms.Label();
            btn_hotkey_exit = new System.Windows.Forms.Button();
            lbl_hotkey_display_profile_description = new System.Windows.Forms.Label();
            btn_hotkey_display_profile = new System.Windows.Forms.Button();
            lbl_hotkey_shortcut_library_description = new System.Windows.Forms.Label();
            btn_hotkey_shortcuts = new System.Windows.Forms.Button();
            lbl_hotkey_exit_app = new System.Windows.Forms.Label();
            lv_hotkeys = new System.Windows.Forms.ListView();
            btn_clear_all_hotkeys = new System.Windows.Forms.Button();
            gb_upgrades = new System.Windows.Forms.GroupBox();
            tlp_upgrades = new System.Windows.Forms.TableLayoutPanel();
            cb_upgrade_enabled = new System.Windows.Forms.CheckBox();
            cb_upgrade_prerelease = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            gb_support = new System.Windows.Forms.GroupBox();
            tlp_support = new System.Windows.Forms.TableLayoutPanel();
            btn_create_support_package = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            flp_bottom = new System.Windows.Forms.FlowLayoutPanel();
            btn_back = new System.Windows.Forms.Button();
            tlpMain.SuspendLayout();
            gb_general.SuspendLayout();
            tlp_general.SuspendLayout();
            tlp_audio_wait.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nud_audio_device_wait).BeginInit();
            tlp_context_menu.SuspendLayout();
            gb_hotkeys.SuspendLayout();
            tlp_hotkeys.SuspendLayout();
            gb_upgrades.SuspendLayout();
            tlp_upgrades.SuspendLayout();
            gb_support.SuspendLayout();
            tlp_support.SuspendLayout();
            flp_bottom.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tlpMain.Controls.Add(gb_general, 0, 0);
            tlpMain.Controls.Add(gb_hotkeys, 1, 0);
            tlpMain.Controls.Add(gb_upgrades, 0, 1);
            tlpMain.Controls.Add(gb_support, 1, 1);
            tlpMain.Controls.Add(flp_bottom, 0, 2);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Location = new System.Drawing.Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new System.Windows.Forms.Padding(9);
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpMain.Size = new System.Drawing.Size(1264, 881);
            tlpMain.TabIndex = 0;
            // 
            // gb_general
            // 
            gb_general.Controls.Add(tlp_general);
            gb_general.Dock = System.Windows.Forms.DockStyle.Fill;
            gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_general.ForeColor = System.Drawing.Color.White;
            gb_general.Location = new System.Drawing.Point(12, 12);
            gb_general.Name = "gb_general";
            gb_general.Size = new System.Drawing.Size(559, 545);
            gb_general.TabIndex = 0;
            gb_general.TabStop = false;
            gb_general.Text = "General Settings";
            // 
            // tlp_general
            // 
            tlp_general.ColumnCount = 1;
            tlp_general.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_general.Controls.Add(cb_start_on_boot, 0, 0);
            tlp_general.Controls.Add(cb_minimise_notification_area, 0, 1);
            tlp_general.Controls.Add(cb_show_splashscreen, 0, 2);
            tlp_general.Controls.Add(cb_show_minimise_action, 0, 3);
            tlp_general.Controls.Add(cb_show_status_action, 0, 4);
            tlp_general.Controls.Add(cb_show_message_toasts, 0, 5);
            tlp_general.Controls.Add(cb_wake_up_gpus, 0, 6);
            tlp_general.Controls.Add(tlp_audio_wait, 0, 7);
            tlp_general.Controls.Add(label6, 0, 8);
            tlp_general.Controls.Add(cmb_notify_icon_double_click, 0, 9);
            tlp_general.Controls.Add(label1, 0, 10);
            tlp_general.Controls.Add(cmb_loglevel, 0, 11);
            tlp_general.Controls.Add(label4, 0, 12);
            tlp_general.Controls.Add(tlp_context_menu, 0, 13);
            tlp_general.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_general.Location = new System.Drawing.Point(3, 26);
            tlp_general.Name = "tlp_general";
            tlp_general.Padding = new System.Windows.Forms.Padding(6);
            tlp_general.RowCount = 14;
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_general.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_general.Size = new System.Drawing.Size(553, 516);
            tlp_general.TabIndex = 0;
            // 
            // cb_start_on_boot
            // 
            cb_start_on_boot.AutoSize = true;
            cb_start_on_boot.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_start_on_boot.ForeColor = System.Drawing.Color.White;
            cb_start_on_boot.Location = new System.Drawing.Point(9, 9);
            cb_start_on_boot.Name = "cb_start_on_boot";
            cb_start_on_boot.Size = new System.Drawing.Size(745, 34);
            cb_start_on_boot.TabIndex = 0;
            cb_start_on_boot.Text = "Start DisplayMagician automatically when the computer starts";
            cb_start_on_boot.UseVisualStyleBackColor = true;
            // 
            // cb_minimise_notification_area
            // 
            cb_minimise_notification_area.AutoSize = true;
            cb_minimise_notification_area.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            cb_minimise_notification_area.Location = new System.Drawing.Point(9, 49);
            cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            cb_minimise_notification_area.Size = new System.Drawing.Size(632, 34);
            cb_minimise_notification_area.TabIndex = 1;
            cb_minimise_notification_area.Text = "Start DisplayMagician minimised in notification area";
            cb_minimise_notification_area.UseVisualStyleBackColor = true;
            // 
            // cb_show_splashscreen
            // 
            cb_show_splashscreen.AutoSize = true;
            cb_show_splashscreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_splashscreen.ForeColor = System.Drawing.Color.White;
            cb_show_splashscreen.Location = new System.Drawing.Point(9, 89);
            cb_show_splashscreen.Name = "cb_show_splashscreen";
            cb_show_splashscreen.Size = new System.Drawing.Size(589, 34);
            cb_show_splashscreen.TabIndex = 2;
            cb_show_splashscreen.Text = "Show DisplayMagician splash screen on startup";
            cb_show_splashscreen.UseVisualStyleBackColor = true;
            // 
            // cb_show_minimise_action
            // 
            cb_show_minimise_action.AutoSize = true;
            cb_show_minimise_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_minimise_action.ForeColor = System.Drawing.Color.White;
            cb_show_minimise_action.Location = new System.Drawing.Point(9, 129);
            cb_show_minimise_action.Name = "cb_show_minimise_action";
            cb_show_minimise_action.Size = new System.Drawing.Size(939, 34);
            cb_show_minimise_action.TabIndex = 3;
            cb_show_minimise_action.Text = "Show reminder in Windows Action Center when DisplayMagician  is minimised";
            cb_show_minimise_action.UseVisualStyleBackColor = true;
            // 
            // cb_show_status_action
            // 
            cb_show_status_action.AutoSize = true;
            cb_show_status_action.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_status_action.ForeColor = System.Drawing.Color.White;
            cb_show_status_action.Location = new System.Drawing.Point(9, 169);
            cb_show_status_action.Name = "cb_show_status_action";
            cb_show_status_action.Size = new System.Drawing.Size(699, 34);
            cb_show_status_action.TabIndex = 4;
            cb_show_status_action.Text = "Show status change messages in Windows Action Center";
            cb_show_status_action.UseVisualStyleBackColor = true;
            // 
            // cb_show_message_toasts
            // 
            cb_show_message_toasts.AutoSize = true;
            cb_show_message_toasts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_show_message_toasts.ForeColor = System.Drawing.Color.White;
            cb_show_message_toasts.Location = new System.Drawing.Point(9, 209);
            cb_show_message_toasts.Name = "cb_show_message_toasts";
            cb_show_message_toasts.Size = new System.Drawing.Size(717, 34);
            cb_show_message_toasts.TabIndex = 5;
            cb_show_message_toasts.Text = "Show new message notifications in Windows Action Center";
            cb_show_message_toasts.UseVisualStyleBackColor = true;
            // 
            // cb_wake_up_gpus
            // 
            cb_wake_up_gpus.AutoSize = true;
            cb_wake_up_gpus.Checked = true;
            cb_wake_up_gpus.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_wake_up_gpus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_wake_up_gpus.ForeColor = System.Drawing.Color.White;
            cb_wake_up_gpus.Location = new System.Drawing.Point(9, 249);
            cb_wake_up_gpus.Name = "cb_wake_up_gpus";
            cb_wake_up_gpus.Size = new System.Drawing.Size(728, 34);
            cb_wake_up_gpus.TabIndex = 6;
            cb_wake_up_gpus.Text = "Keep GPUs awake to make laptops display changes reliable";
            cb_wake_up_gpus.UseVisualStyleBackColor = true;
            // 
            // tlp_audio_wait
            // 
            tlp_audio_wait.AutoSize = true;
            tlp_audio_wait.ColumnCount = 3;
            tlp_audio_wait.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_audio_wait.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_audio_wait.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_audio_wait.Controls.Add(label5, 0, 0);
            tlp_audio_wait.Controls.Add(nud_audio_device_wait, 1, 0);
            tlp_audio_wait.Controls.Add(label7, 2, 0);
            tlp_audio_wait.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_audio_wait.Location = new System.Drawing.Point(9, 289);
            tlp_audio_wait.Name = "tlp_audio_wait";
            tlp_audio_wait.RowCount = 1;
            tlp_audio_wait.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_audio_wait.Size = new System.Drawing.Size(535, 44);
            tlp_audio_wait.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label5.ForeColor = System.Drawing.Color.Transparent;
            label5.Location = new System.Drawing.Point(3, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(744, 30);
            label5.TabIndex = 0;
            label5.Text = "Max time to wait for audio device to appear (default 10 seconds):";
            // 
            // nud_audio_device_wait
            // 
            nud_audio_device_wait.Location = new System.Drawing.Point(753, 3);
            nud_audio_device_wait.Maximum = new decimal(new int[] { 45, 0, 0, 0 });
            nud_audio_device_wait.Name = "nud_audio_device_wait";
            nud_audio_device_wait.Size = new System.Drawing.Size(111, 37);
            nud_audio_device_wait.TabIndex = 1;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label7.ForeColor = System.Drawing.Color.Transparent;
            label7.Location = new System.Drawing.Point(870, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(108, 30);
            label7.TabIndex = 2;
            label7.Text = "seconds";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label6.ForeColor = System.Drawing.Color.Transparent;
            label6.Location = new System.Drawing.Point(9, 336);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(594, 30);
            label6.TabIndex = 8;
            label6.Text = "Notification Icon (System Tray) double click action :";
            // 
            // cmb_notify_icon_double_click
            // 
            cmb_notify_icon_double_click.Dock = System.Windows.Forms.DockStyle.Fill;
            cmb_notify_icon_double_click.FormattingEnabled = true;
            cmb_notify_icon_double_click.Location = new System.Drawing.Point(9, 369);
            cmb_notify_icon_double_click.Name = "cmb_notify_icon_double_click";
            cmb_notify_icon_double_click.Size = new System.Drawing.Size(535, 38);
            cmb_notify_icon_double_click.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.Transparent;
            label1.Location = new System.Drawing.Point(9, 410);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(259, 30);
            label1.TabIndex = 10;
            label1.Text = "What type of logging?";
            // 
            // cmb_loglevel
            // 
            cmb_loglevel.Dock = System.Windows.Forms.DockStyle.Fill;
            cmb_loglevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cmb_loglevel.FormattingEnabled = true;
            cmb_loglevel.Location = new System.Drawing.Point(9, 443);
            cmb_loglevel.Name = "cmb_loglevel";
            cmb_loglevel.Size = new System.Drawing.Size(535, 38);
            cmb_loglevel.TabIndex = 11;
            // 
            // label4
            // 
            label4.Dock = System.Windows.Forms.DockStyle.Fill;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label4.Location = new System.Drawing.Point(9, 484);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(535, 26);
            label4.TabIndex = 12;
            label4.Text = "Add or remove the Desktop Background Context Menu. ";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlp_context_menu
            // 
            tlp_context_menu.AutoSize = true;
            tlp_context_menu.ColumnCount = 2;
            tlp_context_menu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp_context_menu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp_context_menu.Controls.Add(btn_context_menu_reinstall, 0, 0);
            tlp_context_menu.Controls.Add(btn_context_menu_uninstall, 1, 0);
            tlp_context_menu.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_context_menu.Location = new System.Drawing.Point(9, 0);
            tlp_context_menu.Margin = new System.Windows.Forms.Padding(0);
            tlp_context_menu.Name = "tlp_context_menu";
            tlp_context_menu.RowCount = 1;
            tlp_context_menu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_context_menu.Size = new System.Drawing.Size(535, 42);
            tlp_context_menu.TabIndex = 13;
            // 
            // btn_context_menu_reinstall
            // 
            btn_context_menu_reinstall.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_context_menu_reinstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_context_menu_reinstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_context_menu_reinstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_context_menu_reinstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_context_menu_reinstall.ForeColor = System.Drawing.Color.White;
            btn_context_menu_reinstall.Location = new System.Drawing.Point(3, 3);
            btn_context_menu_reinstall.Name = "btn_context_menu_reinstall";
            btn_context_menu_reinstall.Size = new System.Drawing.Size(261, 36);
            btn_context_menu_reinstall.TabIndex = 0;
            btn_context_menu_reinstall.Text = "Add Desktop Context Menu";
            btn_context_menu_reinstall.UseVisualStyleBackColor = true;
            btn_context_menu_reinstall.Click += btn_context_menu_add_Click;
            // 
            // btn_context_menu_uninstall
            // 
            btn_context_menu_uninstall.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_context_menu_uninstall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_context_menu_uninstall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_context_menu_uninstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_context_menu_uninstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_context_menu_uninstall.ForeColor = System.Drawing.Color.White;
            btn_context_menu_uninstall.Location = new System.Drawing.Point(270, 3);
            btn_context_menu_uninstall.Name = "btn_context_menu_uninstall";
            btn_context_menu_uninstall.Size = new System.Drawing.Size(262, 36);
            btn_context_menu_uninstall.TabIndex = 1;
            btn_context_menu_uninstall.Text = "Remove Desktop Context Menu";
            btn_context_menu_uninstall.UseVisualStyleBackColor = true;
            btn_context_menu_uninstall.Click += btn_context_menu_remove_Click;
            // 
            // gb_hotkeys
            // 
            gb_hotkeys.Controls.Add(tlp_hotkeys);
            gb_hotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            gb_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_hotkeys.ForeColor = System.Drawing.Color.White;
            gb_hotkeys.Location = new System.Drawing.Point(577, 12);
            gb_hotkeys.Name = "gb_hotkeys";
            gb_hotkeys.Size = new System.Drawing.Size(675, 545);
            gb_hotkeys.TabIndex = 1;
            gb_hotkeys.TabStop = false;
            gb_hotkeys.Text = "Hotkeys";
            // 
            // tlp_hotkeys
            // 
            tlp_hotkeys.ColumnCount = 2;
            tlp_hotkeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlp_hotkeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlp_hotkeys.Controls.Add(lbl_hotkey_main_window_description, 0, 0);
            tlp_hotkeys.Controls.Add(btn_hotkey_main_window, 1, 0);
            tlp_hotkeys.Controls.Add(lbl_hotkey_exit, 0, 1);
            tlp_hotkeys.Controls.Add(btn_hotkey_exit, 1, 1);
            tlp_hotkeys.Controls.Add(lbl_hotkey_display_profile_description, 0, 2);
            tlp_hotkeys.Controls.Add(btn_hotkey_display_profile, 1, 2);
            tlp_hotkeys.Controls.Add(lbl_hotkey_shortcut_library_description, 0, 3);
            tlp_hotkeys.Controls.Add(btn_hotkey_shortcuts, 1, 3);
            tlp_hotkeys.Controls.Add(lbl_hotkey_exit_app, 0, 4);
            tlp_hotkeys.Controls.Add(lv_hotkeys, 0, 5);
            tlp_hotkeys.Controls.Add(btn_clear_all_hotkeys, 0, 6);
            tlp_hotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_hotkeys.Location = new System.Drawing.Point(3, 26);
            tlp_hotkeys.Name = "tlp_hotkeys";
            tlp_hotkeys.Padding = new System.Windows.Forms.Padding(6);
            tlp_hotkeys.RowCount = 7;
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_hotkeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_hotkeys.Size = new System.Drawing.Size(669, 516);
            tlp_hotkeys.TabIndex = 0;
            // 
            // lbl_hotkey_main_window_description
            // 
            lbl_hotkey_main_window_description.AutoSize = true;
            lbl_hotkey_main_window_description.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_main_window_description.Location = new System.Drawing.Point(9, 6);
            lbl_hotkey_main_window_description.Name = "lbl_hotkey_main_window_description";
            lbl_hotkey_main_window_description.Size = new System.Drawing.Size(393, 58);
            lbl_hotkey_main_window_description.TabIndex = 0;
            lbl_hotkey_main_window_description.Text = "Hotkey to open Main Window:";
            lbl_hotkey_main_window_description.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_hotkey_main_window
            // 
            btn_hotkey_main_window.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_hotkey_main_window.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_main_window.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_main_window.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_main_window.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_main_window.ForeColor = System.Drawing.Color.White;
            btn_hotkey_main_window.Location = new System.Drawing.Point(408, 9);
            btn_hotkey_main_window.Name = "btn_hotkey_main_window";
            btn_hotkey_main_window.Size = new System.Drawing.Size(252, 52);
            btn_hotkey_main_window.TabIndex = 1;
            btn_hotkey_main_window.Text = "Set Hotkey";
            btn_hotkey_main_window.UseVisualStyleBackColor = true;
            btn_hotkey_main_window.Click += btn_hotkey_main_window_Click;
            // 
            // lbl_hotkey_exit
            // 
            lbl_hotkey_exit.AutoSize = true;
            lbl_hotkey_exit.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_exit.Location = new System.Drawing.Point(9, 64);
            lbl_hotkey_exit.Name = "lbl_hotkey_exit";
            lbl_hotkey_exit.Size = new System.Drawing.Size(393, 58);
            lbl_hotkey_exit.TabIndex = 2;
            lbl_hotkey_exit.Text = "Hotkey to exit DisplayMagician:";
            lbl_hotkey_exit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl_hotkey_exit.Click += lbl_hotkey_exit_Click;
            // 
            // btn_hotkey_exit
            // 
            btn_hotkey_exit.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_hotkey_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_exit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_exit.ForeColor = System.Drawing.Color.White;
            btn_hotkey_exit.Location = new System.Drawing.Point(408, 67);
            btn_hotkey_exit.Name = "btn_hotkey_exit";
            btn_hotkey_exit.Size = new System.Drawing.Size(252, 52);
            btn_hotkey_exit.TabIndex = 3;
            btn_hotkey_exit.Text = "Set Hotkey";
            btn_hotkey_exit.UseVisualStyleBackColor = true;
            btn_hotkey_exit.Click += btn_hotkey_exit_app_Click;
            // 
            // lbl_hotkey_display_profile_description
            // 
            lbl_hotkey_display_profile_description.AutoSize = true;
            lbl_hotkey_display_profile_description.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_display_profile_description.Location = new System.Drawing.Point(9, 122);
            lbl_hotkey_display_profile_description.Name = "lbl_hotkey_display_profile_description";
            lbl_hotkey_display_profile_description.Size = new System.Drawing.Size(393, 58);
            lbl_hotkey_display_profile_description.TabIndex = 4;
            lbl_hotkey_display_profile_description.Text = "Hotkey to open Display Profile Window:";
            lbl_hotkey_display_profile_description.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_hotkey_display_profile
            // 
            btn_hotkey_display_profile.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_hotkey_display_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_display_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_display_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_display_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_display_profile.ForeColor = System.Drawing.Color.White;
            btn_hotkey_display_profile.Location = new System.Drawing.Point(408, 125);
            btn_hotkey_display_profile.Name = "btn_hotkey_display_profile";
            btn_hotkey_display_profile.Size = new System.Drawing.Size(252, 52);
            btn_hotkey_display_profile.TabIndex = 5;
            btn_hotkey_display_profile.Text = "Set Hotkey";
            btn_hotkey_display_profile.UseVisualStyleBackColor = true;
            btn_hotkey_display_profile.Click += btn_hotkey_display_profile_Click;
            // 
            // lbl_hotkey_shortcut_library_description
            // 
            lbl_hotkey_shortcut_library_description.AutoSize = true;
            lbl_hotkey_shortcut_library_description.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_shortcut_library_description.Location = new System.Drawing.Point(9, 180);
            lbl_hotkey_shortcut_library_description.Name = "lbl_hotkey_shortcut_library_description";
            lbl_hotkey_shortcut_library_description.Size = new System.Drawing.Size(393, 58);
            lbl_hotkey_shortcut_library_description.TabIndex = 6;
            lbl_hotkey_shortcut_library_description.Text = "Hotkey to open Shortcut Library:";
            lbl_hotkey_shortcut_library_description.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_hotkey_shortcuts
            // 
            btn_hotkey_shortcuts.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_hotkey_shortcuts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey_shortcuts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey_shortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey_shortcuts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey_shortcuts.ForeColor = System.Drawing.Color.White;
            btn_hotkey_shortcuts.Location = new System.Drawing.Point(408, 183);
            btn_hotkey_shortcuts.Name = "btn_hotkey_shortcuts";
            btn_hotkey_shortcuts.Size = new System.Drawing.Size(252, 52);
            btn_hotkey_shortcuts.TabIndex = 7;
            btn_hotkey_shortcuts.Text = "Set Hotkey";
            btn_hotkey_shortcuts.UseVisualStyleBackColor = true;
            btn_hotkey_shortcuts.Click += btn_hotkey_shortcuts_Click;
            // 
            // lbl_hotkey_exit_app
            // 
            tlp_hotkeys.SetColumnSpan(lbl_hotkey_exit_app, 2);
            lbl_hotkey_exit_app.AutoSize = true;
            lbl_hotkey_exit_app.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_exit_app.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_exit_app.Location = new System.Drawing.Point(9, 241);
            lbl_hotkey_exit_app.Name = "lbl_hotkey_exit_app";
            lbl_hotkey_exit_app.Size = new System.Drawing.Size(651, 37);
            lbl_hotkey_exit_app.TabIndex = 8;
            lbl_hotkey_exit_app.Text = "All Saved Hotkeys";
            lbl_hotkey_exit_app.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lv_hotkeys
            // 
            tlp_hotkeys.SetColumnSpan(lv_hotkeys, 2);
            lv_hotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            lv_hotkeys.Location = new System.Drawing.Point(9, 281);
            lv_hotkeys.Name = "lv_hotkeys";
            lv_hotkeys.ShowGroups = false;
            lv_hotkeys.Size = new System.Drawing.Size(651, 186);
            lv_hotkeys.TabIndex = 9;
            lv_hotkeys.UseCompatibleStateImageBehavior = false;
            lv_hotkeys.View = System.Windows.Forms.View.Details;
            lv_hotkeys.MouseClick += lv_hotkeys_MouseClick;
            // 
            // btn_clear_all_hotkeys
            // 
            tlp_hotkeys.SetColumnSpan(btn_clear_all_hotkeys, 2);
            btn_clear_all_hotkeys.AutoSize = true;
            btn_clear_all_hotkeys.Dock = System.Windows.Forms.DockStyle.Bottom;
            btn_clear_all_hotkeys.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear_all_hotkeys.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear_all_hotkeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear_all_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_clear_all_hotkeys.ForeColor = System.Drawing.Color.White;
            btn_clear_all_hotkeys.Location = new System.Drawing.Point(9, 473);
            btn_clear_all_hotkeys.Name = "btn_clear_all_hotkeys";
            btn_clear_all_hotkeys.Size = new System.Drawing.Size(651, 34);
            btn_clear_all_hotkeys.TabIndex = 10;
            btn_clear_all_hotkeys.Text = "Clear All Hotkeys";
            btn_clear_all_hotkeys.UseVisualStyleBackColor = true;
            btn_clear_all_hotkeys.Click += btn_clear_all_hotkeys_Click;
            // 
            // gb_upgrades
            // 
            gb_upgrades.Controls.Add(tlp_upgrades);
            gb_upgrades.Dock = System.Windows.Forms.DockStyle.Fill;
            gb_upgrades.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_upgrades.ForeColor = System.Drawing.Color.White;
            gb_upgrades.Location = new System.Drawing.Point(12, 563);
            gb_upgrades.Name = "gb_upgrades";
            gb_upgrades.Size = new System.Drawing.Size(559, 290);
            gb_upgrades.TabIndex = 2;
            gb_upgrades.TabStop = false;
            gb_upgrades.Text = "Upgrade Settings";
            // 
            // tlp_upgrades
            // 
            tlp_upgrades.ColumnCount = 1;
            tlp_upgrades.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_upgrades.Controls.Add(cb_upgrade_enabled, 0, 0);
            tlp_upgrades.Controls.Add(cb_upgrade_prerelease, 0, 1);
            tlp_upgrades.Controls.Add(label2, 0, 2);
            tlp_upgrades.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_upgrades.Location = new System.Drawing.Point(3, 26);
            tlp_upgrades.Name = "tlp_upgrades";
            tlp_upgrades.Padding = new System.Windows.Forms.Padding(6);
            tlp_upgrades.RowCount = 3;
            tlp_upgrades.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upgrades.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upgrades.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_upgrades.Size = new System.Drawing.Size(553, 261);
            tlp_upgrades.TabIndex = 0;
            // 
            // cb_upgrade_enabled
            // 
            cb_upgrade_enabled.AutoSize = true;
            cb_upgrade_enabled.Checked = true;
            cb_upgrade_enabled.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_upgrade_enabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_upgrade_enabled.ForeColor = System.Drawing.Color.White;
            cb_upgrade_enabled.Location = new System.Drawing.Point(9, 9);
            cb_upgrade_enabled.Name = "cb_upgrade_enabled";
            cb_upgrade_enabled.Size = new System.Drawing.Size(756, 34);
            cb_upgrade_enabled.TabIndex = 0;
            cb_upgrade_enabled.Text = "Check if new DisplayMagician updates are available on startup";
            cb_upgrade_enabled.UseVisualStyleBackColor = true;
            // 
            // cb_upgrade_prerelease
            // 
            cb_upgrade_prerelease.AutoSize = true;
            cb_upgrade_prerelease.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cb_upgrade_prerelease.ForeColor = System.Drawing.Color.White;
            cb_upgrade_prerelease.Location = new System.Drawing.Point(9, 49);
            cb_upgrade_prerelease.Name = "cb_upgrade_prerelease";
            cb_upgrade_prerelease.Size = new System.Drawing.Size(769, 34);
            cb_upgrade_prerelease.TabIndex = 1;
            cb_upgrade_prerelease.Text = "Upgrade DisplayMagician to latest beta versions when available";
            cb_upgrade_prerelease.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(9, 86);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(835, 26);
            label2.TabIndex = 2;
            label2.Text = "(NOTE: beta versions may crash and not work correctly. Use this option with caution!)";
            // 
            // gb_support
            // 
            gb_support.Controls.Add(tlp_support);
            gb_support.Dock = System.Windows.Forms.DockStyle.Fill;
            gb_support.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_support.ForeColor = System.Drawing.Color.White;
            gb_support.Location = new System.Drawing.Point(577, 563);
            gb_support.Name = "gb_support";
            gb_support.Size = new System.Drawing.Size(675, 290);
            gb_support.TabIndex = 3;
            gb_support.TabStop = false;
            gb_support.Text = "Support Settings";
            // 
            // tlp_support
            // 
            tlp_support.ColumnCount = 1;
            tlp_support.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_support.Controls.Add(btn_create_support_package, 0, 0);
            tlp_support.Controls.Add(label3, 0, 1);
            tlp_support.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_support.Location = new System.Drawing.Point(3, 26);
            tlp_support.Name = "tlp_support";
            tlp_support.Padding = new System.Windows.Forms.Padding(6);
            tlp_support.RowCount = 2;
            tlp_support.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_support.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_support.Size = new System.Drawing.Size(669, 261);
            tlp_support.TabIndex = 0;
            // 
            // btn_create_support_package
            // 
            btn_create_support_package.AutoSize = true;
            btn_create_support_package.Dock = System.Windows.Forms.DockStyle.Top;
            btn_create_support_package.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_create_support_package.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_create_support_package.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_create_support_package.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_create_support_package.ForeColor = System.Drawing.Color.White;
            btn_create_support_package.Location = new System.Drawing.Point(9, 9);
            btn_create_support_package.Name = "btn_create_support_package";
            btn_create_support_package.Size = new System.Drawing.Size(651, 50);
            btn_create_support_package.TabIndex = 0;
            btn_create_support_package.Text = "Create a Support Zip File";
            btn_create_support_package.UseVisualStyleBackColor = true;
            btn_create_support_package.Click += btn_create_support_package_Click;
            // 
            // label3
            // 
            label3.Dock = System.Windows.Forms.DockStyle.Fill;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label3.Location = new System.Drawing.Point(9, 62);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(651, 193);
            label3.TabIndex = 1;
            label3.Text = "Use this button to save a support zip file to your computer. You can then upload this file to GitHub when you have a problem you need me to fix";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flp_bottom
            // 
            tlpMain.SetColumnSpan(flp_bottom, 2);
            flp_bottom.Controls.Add(btn_back);
            flp_bottom.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flp_bottom.Location = new System.Drawing.Point(12, 859);
            flp_bottom.Name = "flp_bottom";
            flp_bottom.Size = new System.Drawing.Size(1240, 10);
            flp_bottom.TabIndex = 4;
            // 
            // btn_back
            // 
            btn_back.AutoSize = true;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1156, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(81, 42);
            btn_back.TabIndex = 0;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1264, 881);
            Controls.Add(tlpMain);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            MinimumSize = new System.Drawing.Size(1200, 820);
            Name = "SettingsForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Settings";
            TopMost = true;
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            tlpMain.ResumeLayout(false);
            gb_general.ResumeLayout(false);
            tlp_general.ResumeLayout(false);
            tlp_general.PerformLayout();
            tlp_audio_wait.ResumeLayout(false);
            tlp_audio_wait.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nud_audio_device_wait).EndInit();
            tlp_context_menu.ResumeLayout(false);
            tlp_context_menu.PerformLayout();
            gb_hotkeys.ResumeLayout(false);
            tlp_hotkeys.ResumeLayout(false);
            tlp_hotkeys.PerformLayout();
            gb_upgrades.ResumeLayout(false);
            tlp_upgrades.ResumeLayout(false);
            tlp_upgrades.PerformLayout();
            gb_support.ResumeLayout(false);
            tlp_support.ResumeLayout(false);
            tlp_support.PerformLayout();
            flp_bottom.ResumeLayout(false);
            flp_bottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.TableLayoutPanel tlp_general;
        private System.Windows.Forms.CheckBox cb_start_on_boot;
        private System.Windows.Forms.CheckBox cb_minimise_notification_area;
        private System.Windows.Forms.CheckBox cb_show_splashscreen;
        private System.Windows.Forms.CheckBox cb_show_minimise_action;
        private System.Windows.Forms.CheckBox cb_show_status_action;
        private System.Windows.Forms.CheckBox cb_show_message_toasts;
        private System.Windows.Forms.CheckBox cb_wake_up_gpus;
        private System.Windows.Forms.TableLayoutPanel tlp_audio_wait;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nud_audio_device_wait;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmb_notify_icon_double_click;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_loglevel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tlp_context_menu;
        private System.Windows.Forms.Button btn_context_menu_reinstall;
        private System.Windows.Forms.Button btn_context_menu_uninstall;
        private System.Windows.Forms.GroupBox gb_hotkeys;
        private System.Windows.Forms.TableLayoutPanel tlp_hotkeys;
        private System.Windows.Forms.Label lbl_hotkey_main_window_description;
        private System.Windows.Forms.Button btn_hotkey_main_window;
        private System.Windows.Forms.Label lbl_hotkey_exit;
        private System.Windows.Forms.Button btn_hotkey_exit;
        private System.Windows.Forms.Label lbl_hotkey_display_profile_description;
        private System.Windows.Forms.Button btn_hotkey_display_profile;
        private System.Windows.Forms.Label lbl_hotkey_shortcut_library_description;
        private System.Windows.Forms.Button btn_hotkey_shortcuts;
        private System.Windows.Forms.Label lbl_hotkey_exit_app;
        private System.Windows.Forms.ListView lv_hotkeys;
        private System.Windows.Forms.Button btn_clear_all_hotkeys;
        private System.Windows.Forms.GroupBox gb_upgrades;
        private System.Windows.Forms.TableLayoutPanel tlp_upgrades;
        private System.Windows.Forms.CheckBox cb_upgrade_enabled;
        private System.Windows.Forms.CheckBox cb_upgrade_prerelease;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gb_support;
        private System.Windows.Forms.TableLayoutPanel tlp_support;
        private System.Windows.Forms.Button btn_create_support_package;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flp_bottom;
        private System.Windows.Forms.Button btn_back;
    }
}