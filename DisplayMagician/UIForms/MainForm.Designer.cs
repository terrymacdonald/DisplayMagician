namespace DisplayMagician.UIForms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            lbl_donate = new System.Windows.Forms.Label();
            btn_fov_calc = new System.Windows.Forms.Button();
            btn_help = new System.Windows.Forms.Button();
            btn_donate = new System.Windows.Forms.Button();
            btn_settings = new System.Windows.Forms.Button();
            lbl_create_profile = new System.Windows.Forms.Label();
            btn_setup_display_profiles = new System.Windows.Forms.Button();
            pb_display_profile = new System.Windows.Forms.PictureBox();
            lbl_create_shortcut = new System.Windows.Forms.Label();
            cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            lbl_version = new System.Windows.Forms.Label();
            btn_setup_game_shortcuts = new System.Windows.Forms.Button();
            btn_exit = new System.Windows.Forms.Button();
            pb_game_shortcut = new System.Windows.Forms.PictureBox();
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            openApplicationWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            profileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            profilesToolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            profileToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            shortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            shortcutsToolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            shortcutToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_display_profile).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pb_game_shortcut).BeginInit();
            mainContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(splitContainer1, "splitContainer1");
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lbl_donate);
            splitContainer1.Panel1.Controls.Add(btn_fov_calc);
            splitContainer1.Panel1.Controls.Add(btn_help);
            splitContainer1.Panel1.Controls.Add(btn_donate);
            splitContainer1.Panel1.Controls.Add(btn_settings);
            splitContainer1.Panel1.Controls.Add(lbl_create_profile);
            splitContainer1.Panel1.Controls.Add(btn_setup_display_profiles);
            splitContainer1.Panel1.Controls.Add(pb_display_profile);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lbl_create_shortcut);
            splitContainer1.Panel2.Controls.Add(cb_minimise_notification_area);
            splitContainer1.Panel2.Controls.Add(lbl_version);
            splitContainer1.Panel2.Controls.Add(btn_setup_game_shortcuts);
            splitContainer1.Panel2.Controls.Add(btn_exit);
            splitContainer1.Panel2.Controls.Add(pb_game_shortcut);
            splitContainer1.TabStop = false;
            // 
            // lbl_donate
            // 
            resources.ApplyResources(lbl_donate, "lbl_donate");
            lbl_donate.BackColor = System.Drawing.Color.Black;
            lbl_donate.ForeColor = System.Drawing.Color.White;
            lbl_donate.Name = "lbl_donate";
            lbl_donate.Click += lbl_donate_Click;
            // 
            // btn_fov_calc
            // 
            resources.ApplyResources(btn_fov_calc, "btn_fov_calc");
            btn_fov_calc.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_fov_calc.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_fov_calc.ForeColor = System.Drawing.Color.White;
            btn_fov_calc.Name = "btn_fov_calc";
            btn_fov_calc.UseVisualStyleBackColor = true;
            btn_fov_calc.Click += btn_fov_calc_Click;
            // 
            // btn_help
            // 
            resources.ApplyResources(btn_help, "btn_help");
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.Name = "btn_help";
            btn_help.UseVisualStyleBackColor = true;
            btn_help.Click += btn_help_Click;
            // 
            // btn_donate
            // 
            resources.ApplyResources(btn_donate, "btn_donate");
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.Name = "btn_donate";
            btn_donate.UseVisualStyleBackColor = true;
            btn_donate.Click += btn_donate_Click;
            // 
            // btn_settings
            // 
            resources.ApplyResources(btn_settings, "btn_settings");
            btn_settings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_settings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_settings.ForeColor = System.Drawing.Color.White;
            btn_settings.Name = "btn_settings";
            btn_settings.UseVisualStyleBackColor = true;
            btn_settings.Click += btn_settings_Click;
            // 
            // lbl_create_profile
            // 
            resources.ApplyResources(lbl_create_profile, "lbl_create_profile");
            lbl_create_profile.BackColor = System.Drawing.Color.Brown;
            lbl_create_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_create_profile.ForeColor = System.Drawing.Color.White;
            lbl_create_profile.Name = "lbl_create_profile";
            lbl_create_profile.Click += lbl_create_profile_Click;
            // 
            // btn_setup_display_profiles
            // 
            resources.ApplyResources(btn_setup_display_profiles, "btn_setup_display_profiles");
            btn_setup_display_profiles.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_setup_display_profiles.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_setup_display_profiles.ForeColor = System.Drawing.Color.White;
            btn_setup_display_profiles.Name = "btn_setup_display_profiles";
            btn_setup_display_profiles.UseVisualStyleBackColor = true;
            btn_setup_display_profiles.Click += btn_setup_display_profiles_Click;
            // 
            // pb_display_profile
            // 
            resources.ApplyResources(pb_display_profile, "pb_display_profile");
            pb_display_profile.Name = "pb_display_profile";
            pb_display_profile.TabStop = false;
            pb_display_profile.Click += pb_display_profile_Click;
            // 
            // lbl_create_shortcut
            // 
            resources.ApplyResources(lbl_create_shortcut, "lbl_create_shortcut");
            lbl_create_shortcut.BackColor = System.Drawing.Color.Brown;
            lbl_create_shortcut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_create_shortcut.ForeColor = System.Drawing.Color.White;
            lbl_create_shortcut.Name = "lbl_create_shortcut";
            lbl_create_shortcut.Click += lbl_create_shortcut_Click;
            // 
            // cb_minimise_notification_area
            // 
            resources.ApplyResources(cb_minimise_notification_area, "cb_minimise_notification_area");
            cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            cb_minimise_notification_area.UseVisualStyleBackColor = true;
            cb_minimise_notification_area.CheckedChanged += cb_minimise_notification_area_CheckedChanged;
            // 
            // lbl_version
            // 
            resources.ApplyResources(lbl_version, "lbl_version");
            lbl_version.BackColor = System.Drawing.Color.Transparent;
            lbl_version.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbl_version.ForeColor = System.Drawing.Color.White;
            lbl_version.Name = "lbl_version";
            // 
            // btn_setup_game_shortcuts
            // 
            resources.ApplyResources(btn_setup_game_shortcuts, "btn_setup_game_shortcuts");
            btn_setup_game_shortcuts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_setup_game_shortcuts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_setup_game_shortcuts.ForeColor = System.Drawing.Color.Transparent;
            btn_setup_game_shortcuts.Name = "btn_setup_game_shortcuts";
            btn_setup_game_shortcuts.UseVisualStyleBackColor = true;
            btn_setup_game_shortcuts.Click += btn_setup_game_shortcuts_Click;
            // 
            // btn_exit
            // 
            resources.ApplyResources(btn_exit, "btn_exit");
            btn_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_exit.ForeColor = System.Drawing.Color.White;
            btn_exit.Name = "btn_exit";
            btn_exit.UseVisualStyleBackColor = true;
            btn_exit.Click += btn_exit_Click;
            // 
            // pb_game_shortcut
            // 
            resources.ApplyResources(pb_game_shortcut, "pb_game_shortcut");
            pb_game_shortcut.Name = "pb_game_shortcut";
            pb_game_shortcut.TabStop = false;
            pb_game_shortcut.Click += pb_game_shortcut_Click;
            // 
            // notifyIcon
            // 
            resources.ApplyResources(notifyIcon, "notifyIcon");
            notifyIcon.ContextMenuStrip = mainContextMenuStrip;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // 
            // mainContextMenuStrip
            // 
            mainContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItemHeading, toolStripSeparator, openApplicationWindowToolStripMenuItem, profileToolStripMenuItem, shortcutToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            mainContextMenuStrip.Name = "mainContextMenuStrip";
            resources.ApplyResources(mainContextMenuStrip, "mainContextMenuStrip");
            // 
            // toolStripMenuItemHeading
            // 
            resources.ApplyResources(toolStripMenuItemHeading, "toolStripMenuItemHeading");
            toolStripMenuItemHeading.Name = "toolStripMenuItemHeading";
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            resources.ApplyResources(toolStripSeparator, "toolStripSeparator");
            // 
            // openApplicationWindowToolStripMenuItem
            // 
            resources.ApplyResources(openApplicationWindowToolStripMenuItem, "openApplicationWindowToolStripMenuItem");
            openApplicationWindowToolStripMenuItem.Name = "openApplicationWindowToolStripMenuItem";
            openApplicationWindowToolStripMenuItem.Click += openApplicationWindowToolStripMenuItem_Click;
            // 
            // profileToolStripMenuItem
            // 
            profileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { profilesToolStripMenuItemHeading, profileToolStripSeparator });
            profileToolStripMenuItem.Name = "profileToolStripMenuItem";
            resources.ApplyResources(profileToolStripMenuItem, "profileToolStripMenuItem");
            // 
            // profilesToolStripMenuItemHeading
            // 
            resources.ApplyResources(profilesToolStripMenuItemHeading, "profilesToolStripMenuItemHeading");
            profilesToolStripMenuItemHeading.Name = "profilesToolStripMenuItemHeading";
            // 
            // profileToolStripSeparator
            // 
            profileToolStripSeparator.Name = "profileToolStripSeparator";
            resources.ApplyResources(profileToolStripSeparator, "profileToolStripSeparator");
            // 
            // shortcutToolStripMenuItem
            // 
            shortcutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { shortcutsToolStripMenuItemHeading, shortcutToolStripSeparator });
            shortcutToolStripMenuItem.Name = "shortcutToolStripMenuItem";
            resources.ApplyResources(shortcutToolStripMenuItem, "shortcutToolStripMenuItem");
            // 
            // shortcutsToolStripMenuItemHeading
            // 
            resources.ApplyResources(shortcutsToolStripMenuItemHeading, "shortcutsToolStripMenuItemHeading");
            shortcutsToolStripMenuItemHeading.Name = "shortcutsToolStripMenuItemHeading";
            // 
            // shortcutToolStripSeparator
            // 
            shortcutToolStripSeparator.Name = "shortcutToolStripSeparator";
            resources.ApplyResources(shortcutToolStripSeparator, "shortcutToolStripSeparator");
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(exitToolStripMenuItem, "exitToolStripMenuItem");
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            MaximizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            Activated += MainForm_Activated;
            Load += MainForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pb_display_profile).EndInit();
            ((System.ComponentModel.ISupportInitialize)pb_game_shortcut).EndInit();
            mainContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pb_display_profile;
        private System.Windows.Forms.PictureBox pb_game_shortcut;
        private System.Windows.Forms.Button btn_exit;
        private System.Windows.Forms.Button btn_setup_display_profiles;
        private System.Windows.Forms.Button btn_setup_game_shortcuts;
        private System.Windows.Forms.Label lbl_version;
        private System.Windows.Forms.ContextMenuStrip mainContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHeading;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem profileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem profilesToolStripMenuItemHeading;
        private System.Windows.Forms.ToolStripMenuItem shortcutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openApplicationWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator profileToolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem shortcutsToolStripMenuItemHeading;
        private System.Windows.Forms.ToolStripSeparator shortcutToolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.CheckBox cb_minimise_notification_area;
        public System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label lbl_create_profile;
        private System.Windows.Forms.Label lbl_create_shortcut;
        private System.Windows.Forms.Button btn_settings;
        private System.Windows.Forms.Button btn_donate;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.Button btn_fov_calc;
        private System.Windows.Forms.Label lbl_donate;
    }
}