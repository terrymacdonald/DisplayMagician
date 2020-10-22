namespace HeliosPlus.UIForms
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
            this.btn_setup_display_profiles = new System.Windows.Forms.Button();
            this.pb_display_profile = new System.Windows.Forms.PictureBox();
            this.lbl_version = new System.Windows.Forms.Label();
            this.btn_setup_game_shortcuts = new System.Windows.Forms.Button();
            this.btn_exit = new System.Windows.Forms.Button();
            this.pb_game_shortcut = new System.Windows.Forms.PictureBox();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_display_profile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_game_shortcut)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.btn_setup_display_profiles);
            this.splitContainer1.Panel1.Controls.Add(this.pb_display_profile);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbl_version);
            this.splitContainer1.Panel2.Controls.Add(this.btn_setup_game_shortcuts);
            this.splitContainer1.Panel2.Controls.Add(this.btn_exit);
            this.splitContainer1.Panel2.Controls.Add(this.pb_game_shortcut);
            this.splitContainer1.TabStop = false;
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
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_display_profile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_game_shortcut)).EndInit();
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
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}