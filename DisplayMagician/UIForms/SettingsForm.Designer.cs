
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
            this.btn_back = new System.Windows.Forms.Button();
            this.gb_general = new System.Windows.Forms.GroupBox();
            this.cb_start_on_boot = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_loglevel = new System.Windows.Forms.ComboBox();
            this.cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            this.gb_hotkeys = new System.Windows.Forms.GroupBox();
            this.lbl_hotkey_shortcut_library = new System.Windows.Forms.Label();
            this.lbl_hotkey_display_profile = new System.Windows.Forms.Label();
            this.lbl_hotkey_main_window = new System.Windows.Forms.Label();
            this.lv_dynamic_hotkeys = new System.Windows.Forms.ListView();
            this.btn_clear_all_hotkeys = new System.Windows.Forms.Button();
            this.lbl_hotkey_shortcut_library_description = new System.Windows.Forms.Label();
            this.lbl_hotkey_display_profile_description = new System.Windows.Forms.Label();
            this.lbl_hotkey_main_window_description = new System.Windows.Forms.Label();
            this.btn_hotkey_shortcuts = new System.Windows.Forms.Button();
            this.btn_hotkey_display_profile = new System.Windows.Forms.Button();
            this.btn_hotkey_main_window = new System.Windows.Forms.Button();
            this.gb_upgrades = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_upgrade_prerelease = new System.Windows.Forms.CheckBox();
            this.gb_support = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_create_support_package = new System.Windows.Forms.Button();
            this.gb_general.SuspendLayout();
            this.gb_hotkeys.SuspendLayout();
            this.gb_upgrades.SuspendLayout();
            this.gb_support.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(1034, 517);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 9;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // gb_general
            // 
            this.gb_general.Controls.Add(this.cb_start_on_boot);
            this.gb_general.Controls.Add(this.label1);
            this.gb_general.Controls.Add(this.cmb_loglevel);
            this.gb_general.Controls.Add(this.cb_minimise_notification_area);
            this.gb_general.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_general.ForeColor = System.Drawing.Color.White;
            this.gb_general.Location = new System.Drawing.Point(27, 21);
            this.gb_general.Name = "gb_general";
            this.gb_general.Size = new System.Drawing.Size(525, 183);
            this.gb_general.TabIndex = 11;
            this.gb_general.TabStop = false;
            this.gb_general.Text = "General Settings";
            // 
            // cb_start_on_boot
            // 
            this.cb_start_on_boot.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cb_start_on_boot.AutoSize = true;
            this.cb_start_on_boot.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cb_start_on_boot.ForeColor = System.Drawing.Color.White;
            this.cb_start_on_boot.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cb_start_on_boot.Location = new System.Drawing.Point(28, 42);
            this.cb_start_on_boot.Name = "cb_start_on_boot";
            this.cb_start_on_boot.Size = new System.Drawing.Size(389, 20);
            this.cb_start_on_boot.TabIndex = 14;
            this.cb_start_on_boot.Text = "Start DisplayMagician automatically when the computer starts";
            this.cb_start_on_boot.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(26, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 16);
            this.label1.TabIndex = 13;
            this.label1.Text = "What type of logging?";
            // 
            // cmb_loglevel
            // 
            this.cmb_loglevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loglevel.FormattingEnabled = true;
            this.cmb_loglevel.Location = new System.Drawing.Point(169, 117);
            this.cmb_loglevel.Name = "cmb_loglevel";
            this.cmb_loglevel.Size = new System.Drawing.Size(333, 24);
            this.cmb_loglevel.TabIndex = 12;
            // 
            // cb_minimise_notification_area
            // 
            this.cb_minimise_notification_area.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cb_minimise_notification_area.AutoSize = true;
            this.cb_minimise_notification_area.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            this.cb_minimise_notification_area.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cb_minimise_notification_area.Location = new System.Drawing.Point(28, 78);
            this.cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            this.cb_minimise_notification_area.Size = new System.Drawing.Size(332, 20);
            this.cb_minimise_notification_area.TabIndex = 11;
            this.cb_minimise_notification_area.Text = "Start DisplayMagician minimised in notification area";
            this.cb_minimise_notification_area.UseVisualStyleBackColor = true;
            // 
            // gb_hotkeys
            // 
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_shortcut_library);
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_display_profile);
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_main_window);
            this.gb_hotkeys.Controls.Add(this.lv_dynamic_hotkeys);
            this.gb_hotkeys.Controls.Add(this.btn_clear_all_hotkeys);
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_shortcut_library_description);
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_display_profile_description);
            this.gb_hotkeys.Controls.Add(this.lbl_hotkey_main_window_description);
            this.gb_hotkeys.Controls.Add(this.btn_hotkey_shortcuts);
            this.gb_hotkeys.Controls.Add(this.btn_hotkey_display_profile);
            this.gb_hotkeys.Controls.Add(this.btn_hotkey_main_window);
            this.gb_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_hotkeys.ForeColor = System.Drawing.Color.White;
            this.gb_hotkeys.Location = new System.Drawing.Point(584, 21);
            this.gb_hotkeys.Name = "gb_hotkeys";
            this.gb_hotkeys.Size = new System.Drawing.Size(525, 391);
            this.gb_hotkeys.TabIndex = 12;
            this.gb_hotkeys.TabStop = false;
            this.gb_hotkeys.Text = "Hotkeys";
            // 
            // lbl_hotkey_shortcut_library
            // 
            this.lbl_hotkey_shortcut_library.Location = new System.Drawing.Point(236, 133);
            this.lbl_hotkey_shortcut_library.Name = "lbl_hotkey_shortcut_library";
            this.lbl_hotkey_shortcut_library.Size = new System.Drawing.Size(169, 16);
            this.lbl_hotkey_shortcut_library.TabIndex = 47;
            this.lbl_hotkey_shortcut_library.Text = "None Set";
            this.lbl_hotkey_shortcut_library.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_hotkey_shortcut_library.Click += new System.EventHandler(this.lbl_hotkey_shortcut_library_Click);
            // 
            // lbl_hotkey_display_profile
            // 
            this.lbl_hotkey_display_profile.Location = new System.Drawing.Point(236, 83);
            this.lbl_hotkey_display_profile.Name = "lbl_hotkey_display_profile";
            this.lbl_hotkey_display_profile.Size = new System.Drawing.Size(169, 16);
            this.lbl_hotkey_display_profile.TabIndex = 46;
            this.lbl_hotkey_display_profile.Text = "None Set";
            this.lbl_hotkey_display_profile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_hotkey_display_profile.Click += new System.EventHandler(this.lbl_hotkey_display_profile_Click);
            // 
            // lbl_hotkey_main_window
            // 
            this.lbl_hotkey_main_window.Location = new System.Drawing.Point(237, 35);
            this.lbl_hotkey_main_window.Name = "lbl_hotkey_main_window";
            this.lbl_hotkey_main_window.Size = new System.Drawing.Size(169, 16);
            this.lbl_hotkey_main_window.TabIndex = 45;
            this.lbl_hotkey_main_window.Text = "None Set";
            this.lbl_hotkey_main_window.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbl_hotkey_main_window.Click += new System.EventHandler(this.lbl_hotkey_main_window_Click);
            // 
            // lv_dynamic_hotkeys
            // 
            this.lv_dynamic_hotkeys.HideSelection = false;
            this.lv_dynamic_hotkeys.Location = new System.Drawing.Point(27, 177);
            this.lv_dynamic_hotkeys.Name = "lv_dynamic_hotkeys";
            this.lv_dynamic_hotkeys.Size = new System.Drawing.Size(473, 143);
            this.lv_dynamic_hotkeys.TabIndex = 44;
            this.lv_dynamic_hotkeys.UseCompatibleStateImageBehavior = false;
            // 
            // btn_clear_all_hotkeys
            // 
            this.btn_clear_all_hotkeys.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_clear_all_hotkeys.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_clear_all_hotkeys.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_clear_all_hotkeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_clear_all_hotkeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_clear_all_hotkeys.ForeColor = System.Drawing.Color.White;
            this.btn_clear_all_hotkeys.Location = new System.Drawing.Point(197, 334);
            this.btn_clear_all_hotkeys.Name = "btn_clear_all_hotkeys";
            this.btn_clear_all_hotkeys.Size = new System.Drawing.Size(127, 33);
            this.btn_clear_all_hotkeys.TabIndex = 42;
            this.btn_clear_all_hotkeys.Text = "Clear All Hotkeys";
            this.btn_clear_all_hotkeys.UseVisualStyleBackColor = true;
            this.btn_clear_all_hotkeys.Click += new System.EventHandler(this.btn_clear_all_hotkeys_Click);
            // 
            // lbl_hotkey_shortcut_library_description
            // 
            this.lbl_hotkey_shortcut_library_description.AutoSize = true;
            this.lbl_hotkey_shortcut_library_description.Location = new System.Drawing.Point(25, 133);
            this.lbl_hotkey_shortcut_library_description.Name = "lbl_hotkey_shortcut_library_description";
            this.lbl_hotkey_shortcut_library_description.Size = new System.Drawing.Size(197, 16);
            this.lbl_hotkey_shortcut_library_description.TabIndex = 41;
            this.lbl_hotkey_shortcut_library_description.Text = "Hotkey to open Shortcut Library:";
            // 
            // lbl_hotkey_display_profile_description
            // 
            this.lbl_hotkey_display_profile_description.AutoSize = true;
            this.lbl_hotkey_display_profile_description.Location = new System.Drawing.Point(25, 83);
            this.lbl_hotkey_display_profile_description.Name = "lbl_hotkey_display_profile_description";
            this.lbl_hotkey_display_profile_description.Size = new System.Drawing.Size(243, 16);
            this.lbl_hotkey_display_profile_description.TabIndex = 40;
            this.lbl_hotkey_display_profile_description.Text = "Hotkey to open Display Profile Window:";
            // 
            // lbl_hotkey_main_window_description
            // 
            this.lbl_hotkey_main_window_description.AutoSize = true;
            this.lbl_hotkey_main_window_description.Location = new System.Drawing.Point(26, 35);
            this.lbl_hotkey_main_window_description.Name = "lbl_hotkey_main_window_description";
            this.lbl_hotkey_main_window_description.Size = new System.Drawing.Size(185, 16);
            this.lbl_hotkey_main_window_description.TabIndex = 39;
            this.lbl_hotkey_main_window_description.Text = "Hotkey to open Main Window:";
            // 
            // btn_hotkey_shortcuts
            // 
            this.btn_hotkey_shortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_hotkey_shortcuts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_hotkey_shortcuts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_hotkey_shortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_hotkey_shortcuts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_hotkey_shortcuts.ForeColor = System.Drawing.Color.White;
            this.btn_hotkey_shortcuts.Location = new System.Drawing.Point(411, 125);
            this.btn_hotkey_shortcuts.Name = "btn_hotkey_shortcuts";
            this.btn_hotkey_shortcuts.Size = new System.Drawing.Size(88, 33);
            this.btn_hotkey_shortcuts.TabIndex = 38;
            this.btn_hotkey_shortcuts.Text = "Set Hotkey";
            this.btn_hotkey_shortcuts.UseVisualStyleBackColor = true;
            this.btn_hotkey_shortcuts.Click += new System.EventHandler(this.btn_hotkey_shortcuts_Click);
            // 
            // btn_hotkey_display_profile
            // 
            this.btn_hotkey_display_profile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_hotkey_display_profile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_hotkey_display_profile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_hotkey_display_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_hotkey_display_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_hotkey_display_profile.ForeColor = System.Drawing.Color.White;
            this.btn_hotkey_display_profile.Location = new System.Drawing.Point(411, 75);
            this.btn_hotkey_display_profile.Name = "btn_hotkey_display_profile";
            this.btn_hotkey_display_profile.Size = new System.Drawing.Size(89, 33);
            this.btn_hotkey_display_profile.TabIndex = 37;
            this.btn_hotkey_display_profile.Text = "Set Hotkey";
            this.btn_hotkey_display_profile.UseVisualStyleBackColor = true;
            this.btn_hotkey_display_profile.Click += new System.EventHandler(this.btn_hotkey_display_profile_Click);
            // 
            // btn_hotkey_main_window
            // 
            this.btn_hotkey_main_window.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_hotkey_main_window.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_hotkey_main_window.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_hotkey_main_window.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_hotkey_main_window.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_hotkey_main_window.ForeColor = System.Drawing.Color.White;
            this.btn_hotkey_main_window.Location = new System.Drawing.Point(412, 27);
            this.btn_hotkey_main_window.Name = "btn_hotkey_main_window";
            this.btn_hotkey_main_window.Size = new System.Drawing.Size(88, 33);
            this.btn_hotkey_main_window.TabIndex = 36;
            this.btn_hotkey_main_window.Text = "Set Hotkey";
            this.btn_hotkey_main_window.UseVisualStyleBackColor = true;
            this.btn_hotkey_main_window.Click += new System.EventHandler(this.btn_hotkey_main_window_Click);
            // 
            // gb_upgrades
            // 
            this.gb_upgrades.Controls.Add(this.label2);
            this.gb_upgrades.Controls.Add(this.cb_upgrade_prerelease);
            this.gb_upgrades.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_upgrades.ForeColor = System.Drawing.Color.White;
            this.gb_upgrades.Location = new System.Drawing.Point(27, 235);
            this.gb_upgrades.Name = "gb_upgrades";
            this.gb_upgrades.Size = new System.Drawing.Size(525, 177);
            this.gb_upgrades.TabIndex = 13;
            this.gb_upgrades.TabStop = false;
            this.gb_upgrades.Text = "Upgrade Settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(43, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(410, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "(NOTE: beta versions may crash and not work correctly. Use this option with cauti" +
    "on!)";
            // 
            // cb_upgrade_prerelease
            // 
            this.cb_upgrade_prerelease.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cb_upgrade_prerelease.AutoSize = true;
            this.cb_upgrade_prerelease.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cb_upgrade_prerelease.ForeColor = System.Drawing.Color.White;
            this.cb_upgrade_prerelease.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cb_upgrade_prerelease.Location = new System.Drawing.Point(28, 42);
            this.cb_upgrade_prerelease.Name = "cb_upgrade_prerelease";
            this.cb_upgrade_prerelease.Size = new System.Drawing.Size(411, 20);
            this.cb_upgrade_prerelease.TabIndex = 14;
            this.cb_upgrade_prerelease.Text = "Upgrade DisplayMagician to latest beta versions when available";
            this.cb_upgrade_prerelease.UseVisualStyleBackColor = true;
            // 
            // gb_support
            // 
            this.gb_support.Controls.Add(this.btn_create_support_package);
            this.gb_support.Controls.Add(this.label3);
            this.gb_support.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_support.ForeColor = System.Drawing.Color.White;
            this.gb_support.Location = new System.Drawing.Point(27, 442);
            this.gb_support.Name = "gb_support";
            this.gb_support.Size = new System.Drawing.Size(525, 98);
            this.gb_support.TabIndex = 16;
            this.gb_support.TabStop = false;
            this.gb_support.Text = "Support Settings";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(105, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(334, 32);
            this.label3.TabIndex = 15;
            this.label3.Text = "Use this button to save a support package to your computer. You can then upload t" +
    "his file to GitHub when you have a problem";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_create_support_package
            // 
            this.btn_create_support_package.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_create_support_package.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_create_support_package.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_create_support_package.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_create_support_package.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_create_support_package.ForeColor = System.Drawing.Color.White;
            this.btn_create_support_package.Location = new System.Drawing.Point(169, 21);
            this.btn_create_support_package.Name = "btn_create_support_package";
            this.btn_create_support_package.Size = new System.Drawing.Size(183, 33);
            this.btn_create_support_package.TabIndex = 48;
            this.btn_create_support_package.Text = "Create a Support Package";
            this.btn_create_support_package.UseVisualStyleBackColor = true;
            this.btn_create_support_package.Click += new System.EventHandler(this.btn_create_support_package_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1142, 564);
            this.Controls.Add(this.gb_support);
            this.Controls.Add(this.gb_upgrades);
            this.Controls.Add(this.gb_hotkeys);
            this.Controls.Add(this.gb_general);
            this.Controls.Add(this.btn_back);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.gb_general.ResumeLayout(false);
            this.gb_general.PerformLayout();
            this.gb_hotkeys.ResumeLayout(false);
            this.gb_hotkeys.PerformLayout();
            this.gb_upgrades.ResumeLayout(false);
            this.gb_upgrades.PerformLayout();
            this.gb_support.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}