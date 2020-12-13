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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lbl_create_profile = new System.Windows.Forms.Label();
            this.btn_setup_display_profiles = new System.Windows.Forms.Button();
            this.pb_display_profile = new System.Windows.Forms.PictureBox();
            this.lbl_create_shortcut = new System.Windows.Forms.Label();
            this.cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            this.lbl_version = new System.Windows.Forms.Label();
            this.btn_setup_game_shortcuts = new System.Windows.Forms.Button();
            this.btn_exit = new System.Windows.Forms.Button();
            this.pb_game_shortcut = new System.Windows.Forms.PictureBox();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.openApplicationWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.profileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.profilesToolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            this.profileToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.shortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shortcutsToolStripMenuItemHeading = new System.Windows.Forms.ToolStripMenuItem();
            this.shortcutToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_display_profile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_game_shortcut)).BeginInit();
            this.mainContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.lbl_create_profile);
            this.splitContainer1.Panel1.Controls.Add(this.btn_setup_display_profiles);
            this.splitContainer1.Panel1.Controls.Add(this.pb_display_profile);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbl_create_shortcut);
            this.splitContainer1.Panel2.Controls.Add(this.cb_minimise_notification_area);
            this.splitContainer1.Panel2.Controls.Add(this.lbl_version);
            this.splitContainer1.Panel2.Controls.Add(this.btn_setup_game_shortcuts);
            this.splitContainer1.Panel2.Controls.Add(this.btn_exit);
            this.splitContainer1.Panel2.Controls.Add(this.pb_game_shortcut);
            this.splitContainer1.TabStop = false;
            // 
            // lbl_create_profile
            // 
            resources.ApplyResources(this.lbl_create_profile, "lbl_create_profile");
            this.lbl_create_profile.BackColor = System.Drawing.Color.Brown;
            this.lbl_create_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_create_profile.ForeColor = System.Drawing.Color.White;
            this.lbl_create_profile.Name = "lbl_create_profile";
            // 
            // btn_setup_display_profiles
            // 
            resources.ApplyResources(this.btn_setup_display_profiles, "btn_setup_display_profiles");
            this.btn_setup_display_profiles.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_setup_display_profiles.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_setup_display_profiles.ForeColor = System.Drawing.Color.White;
            this.btn_setup_display_profiles.Name = "btn_setup_display_profiles";
            this.btn_setup_display_profiles.UseVisualStyleBackColor = true;
            this.btn_setup_display_profiles.Click += new System.EventHandler(this.btn_setup_display_profiles_Click);
            // 
            // pb_display_profile
            // 
            resources.ApplyResources(this.pb_display_profile, "pb_display_profile");
            this.pb_display_profile.Name = "pb_display_profile";
            this.pb_display_profile.TabStop = false;
            this.pb_display_profile.Click += new System.EventHandler(this.pb_display_profile_Click);
            // 
            // lbl_create_shortcut
            // 
            resources.ApplyResources(this.lbl_create_shortcut, "lbl_create_shortcut");
            this.lbl_create_shortcut.BackColor = System.Drawing.Color.Brown;
            this.lbl_create_shortcut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_create_shortcut.ForeColor = System.Drawing.Color.White;
            this.lbl_create_shortcut.Name = "lbl_create_shortcut";
            // 
            // cb_minimise_notification_area
            // 
            resources.ApplyResources(this.cb_minimise_notification_area, "cb_minimise_notification_area");
            this.cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            this.cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            this.cb_minimise_notification_area.UseVisualStyleBackColor = true;
            this.cb_minimise_notification_area.CheckedChanged += new System.EventHandler(this.cb_minimise_notification_area_CheckedChanged);
            // 
            // lbl_version
            // 
            resources.ApplyResources(this.lbl_version, "lbl_version");
            this.lbl_version.BackColor = System.Drawing.Color.Transparent;
            this.lbl_version.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbl_version.ForeColor = System.Drawing.Color.White;
            this.lbl_version.Name = "lbl_version";
            // 
            // btn_setup_game_shortcuts
            // 
            resources.ApplyResources(this.btn_setup_game_shortcuts, "btn_setup_game_shortcuts");
            this.btn_setup_game_shortcuts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_setup_game_shortcuts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_setup_game_shortcuts.ForeColor = System.Drawing.Color.Transparent;
            this.btn_setup_game_shortcuts.Name = "btn_setup_game_shortcuts";
            this.btn_setup_game_shortcuts.UseVisualStyleBackColor = true;
            this.btn_setup_game_shortcuts.Click += new System.EventHandler(this.btn_setup_game_shortcuts_Click);
            // 
            // btn_exit
            // 
            resources.ApplyResources(this.btn_exit, "btn_exit");
            this.btn_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_exit.ForeColor = System.Drawing.Color.White;
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.UseVisualStyleBackColor = true;
            this.btn_exit.Click += new System.EventHandler(this.btn_exit_Click);
            // 
            // pb_game_shortcut
            // 
            resources.ApplyResources(this.pb_game_shortcut, "pb_game_shortcut");
            this.pb_game_shortcut.Name = "pb_game_shortcut";
            this.pb_game_shortcut.TabStop = false;
            this.pb_game_shortcut.Click += new System.EventHandler(this.pb_game_shortcut_Click);
            // 
            // notifyIcon
            // 
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.ContextMenuStrip = this.mainContextMenuStrip;
            // 
            // mainContextMenuStrip
            // 
            this.mainContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemHeading,
            this.toolStripSeparator,
            this.openApplicationWindowToolStripMenuItem,
            this.profileToolStripMenuItem,
            this.shortcutToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.mainContextMenuStrip.Name = "mainContextMenuStrip";
            resources.ApplyResources(this.mainContextMenuStrip, "mainContextMenuStrip");
            // 
            // toolStripMenuItemHeading
            // 
            resources.ApplyResources(this.toolStripMenuItemHeading, "toolStripMenuItemHeading");
            this.toolStripMenuItemHeading.Name = "toolStripMenuItemHeading";
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            resources.ApplyResources(this.toolStripSeparator, "toolStripSeparator");
            // 
            // openApplicationWindowToolStripMenuItem
            // 
            resources.ApplyResources(this.openApplicationWindowToolStripMenuItem, "openApplicationWindowToolStripMenuItem");
            this.openApplicationWindowToolStripMenuItem.Name = "openApplicationWindowToolStripMenuItem";
            this.openApplicationWindowToolStripMenuItem.Click += new System.EventHandler(this.openApplicationWindowToolStripMenuItem_Click);
            // 
            // profileToolStripMenuItem
            // 
            this.profileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.profilesToolStripMenuItemHeading,
            this.profileToolStripSeparator});
            this.profileToolStripMenuItem.Name = "profileToolStripMenuItem";
            resources.ApplyResources(this.profileToolStripMenuItem, "profileToolStripMenuItem");
            // 
            // profilesToolStripMenuItemHeading
            // 
            resources.ApplyResources(this.profilesToolStripMenuItemHeading, "profilesToolStripMenuItemHeading");
            this.profilesToolStripMenuItemHeading.Name = "profilesToolStripMenuItemHeading";
            // 
            // profileToolStripSeparator
            // 
            this.profileToolStripSeparator.Name = "profileToolStripSeparator";
            resources.ApplyResources(this.profileToolStripSeparator, "profileToolStripSeparator");
            // 
            // shortcutToolStripMenuItem
            // 
            this.shortcutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shortcutsToolStripMenuItemHeading,
            this.shortcutToolStripSeparator});
            this.shortcutToolStripMenuItem.Name = "shortcutToolStripMenuItem";
            resources.ApplyResources(this.shortcutToolStripMenuItem, "shortcutToolStripMenuItem");
            // 
            // shortcutsToolStripMenuItemHeading
            // 
            resources.ApplyResources(this.shortcutsToolStripMenuItemHeading, "shortcutsToolStripMenuItemHeading");
            this.shortcutsToolStripMenuItemHeading.Name = "shortcutsToolStripMenuItemHeading";
            // 
            // shortcutToolStripSeparator
            // 
            this.shortcutToolStripSeparator.Name = "shortcutToolStripSeparator";
            resources.ApplyResources(this.shortcutToolStripSeparator, "shortcutToolStripSeparator");
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            resources.ApplyResources(this.button1, "button1");
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_display_profile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_game_shortcut)).EndInit();
            this.mainContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Button button1;
    }
}