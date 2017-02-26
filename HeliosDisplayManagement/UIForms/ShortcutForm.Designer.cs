using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared.UserControls;

namespace HeliosDisplayManagement.UIForms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutForm));
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.g_temp = new System.Windows.Forms.GroupBox();
            this.p_steam = new System.Windows.Forms.Panel();
            this.nud_steamapps = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.nud_steamtimeout = new System.Windows.Forms.NumericUpDown();
            this.lbl_steamname = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_steamappid = new System.Windows.Forms.NumericUpDown();
            this.p_standalone = new System.Windows.Forms.Panel();
            this.btn_app_executable = new System.Windows.Forms.Button();
            this.txt_executable = new System.Windows.Forms.TextBox();
            this.lbl_app_executable = new System.Windows.Forms.Label();
            this.txt_process = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_process = new System.Windows.Forms.CheckBox();
            this.nud_timeout = new System.Windows.Forms.NumericUpDown();
            this.rb_steam = new System.Windows.Forms.RadioButton();
            this.rb_standalone = new System.Windows.Forms.RadioButton();
            this.txt_args = new System.Windows.Forms.TextBox();
            this.cb_args = new System.Windows.Forms.CheckBox();
            this.cb_temp = new System.Windows.Forms.CheckBox();
            this.dv_profile = new HeliosDisplayManagement.Shared.UserControls.DisplayView();
            this.dialog_open = new System.Windows.Forms.OpenFileDialog();
            this.dialog_save = new System.Windows.Forms.SaveFileDialog();
            this.g_temp.SuspendLayout();
            this.p_steam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_steamtimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_steamappid)).BeginInit();
            this.p_standalone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(551, 526);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(93, 23);
            this.btn_save.TabIndex = 6;
            this.btn_save.Text = global::HeliosDisplayManagement.Resources.Language.Place_Shortcut;
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(452, 526);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(93, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = global::HeliosDisplayManagement.Resources.Language.Cancel;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // g_temp
            // 
            this.g_temp.Controls.Add(this.p_steam);
            this.g_temp.Controls.Add(this.p_standalone);
            this.g_temp.Controls.Add(this.rb_steam);
            this.g_temp.Controls.Add(this.rb_standalone);
            this.g_temp.Controls.Add(this.txt_args);
            this.g_temp.Controls.Add(this.cb_args);
            this.g_temp.Enabled = false;
            this.g_temp.Location = new System.Drawing.Point(12, 332);
            this.g_temp.Name = "g_temp";
            this.g_temp.Size = new System.Drawing.Size(632, 188);
            this.g_temp.TabIndex = 4;
            this.g_temp.TabStop = false;
            this.g_temp.Text = global::HeliosDisplayManagement.Resources.Language.Process_Information;
            // 
            // p_steam
            // 
            this.p_steam.Controls.Add(this.nud_steamapps);
            this.p_steam.Controls.Add(this.label5);
            this.p_steam.Controls.Add(this.nud_steamtimeout);
            this.p_steam.Controls.Add(this.lbl_steamname);
            this.p_steam.Controls.Add(this.label3);
            this.p_steam.Controls.Add(this.nud_steamappid);
            this.p_steam.Enabled = false;
            this.p_steam.Location = new System.Drawing.Point(26, 123);
            this.p_steam.Name = "p_steam";
            this.p_steam.Size = new System.Drawing.Size(600, 26);
            this.p_steam.TabIndex = 3;
            // 
            // nud_steamapps
            // 
            this.nud_steamapps.Location = new System.Drawing.Point(209, 3);
            this.nud_steamapps.Name = "nud_steamapps";
            this.nud_steamapps.Size = new System.Drawing.Size(24, 20);
            this.nud_steamapps.TabIndex = 2;
            this.nud_steamapps.Text = "...";
            this.nud_steamapps.UseVisualStyleBackColor = true;
            this.nud_steamapps.Click += new System.EventHandler(this.nud_steamapps_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(476, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = global::HeliosDisplayManagement.Resources.Language.Timeout;
            // 
            // nud_steamtimeout
            // 
            this.nud_steamtimeout.Location = new System.Drawing.Point(527, 3);
            this.nud_steamtimeout.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_steamtimeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nud_steamtimeout.Name = "nud_steamtimeout";
            this.nud_steamtimeout.Size = new System.Drawing.Size(70, 20);
            this.nud_steamtimeout.TabIndex = 5;
            this.nud_steamtimeout.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // lbl_steamname
            // 
            this.lbl_steamname.Location = new System.Drawing.Point(239, 5);
            this.lbl_steamname.Name = "lbl_steamname";
            this.lbl_steamname.Size = new System.Drawing.Size(231, 15);
            this.lbl_steamname.TabIndex = 3;
            this.lbl_steamname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = global::HeliosDisplayManagement.Resources.Language.Games_AppId;
            // 
            // nud_steamappid
            // 
            this.nud_steamappid.Location = new System.Drawing.Point(114, 3);
            this.nud_steamappid.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.nud_steamappid.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_steamappid.Name = "nud_steamappid";
            this.nud_steamappid.Size = new System.Drawing.Size(89, 20);
            this.nud_steamappid.TabIndex = 1;
            this.nud_steamappid.Value = new decimal(new int[] {
            730,
            0,
            0,
            0});
            this.nud_steamappid.ValueChanged += new System.EventHandler(this.nud_steamappid_ValueChanged);
            // 
            // p_standalone
            // 
            this.p_standalone.Controls.Add(this.btn_app_executable);
            this.p_standalone.Controls.Add(this.txt_executable);
            this.p_standalone.Controls.Add(this.lbl_app_executable);
            this.p_standalone.Controls.Add(this.txt_process);
            this.p_standalone.Controls.Add(this.label2);
            this.p_standalone.Controls.Add(this.cb_process);
            this.p_standalone.Controls.Add(this.nud_timeout);
            this.p_standalone.Location = new System.Drawing.Point(26, 42);
            this.p_standalone.Name = "p_standalone";
            this.p_standalone.Size = new System.Drawing.Size(600, 52);
            this.p_standalone.TabIndex = 1;
            // 
            // btn_app_executable
            // 
            this.btn_app_executable.Location = new System.Drawing.Point(573, 3);
            this.btn_app_executable.Name = "btn_app_executable";
            this.btn_app_executable.Size = new System.Drawing.Size(24, 20);
            this.btn_app_executable.TabIndex = 2;
            this.btn_app_executable.Text = "...";
            this.btn_app_executable.UseVisualStyleBackColor = true;
            this.btn_app_executable.Click += new System.EventHandler(this.btn_app_executable_Click);
            // 
            // txt_executable
            // 
            this.txt_executable.Location = new System.Drawing.Point(114, 3);
            this.txt_executable.Name = "txt_executable";
            this.txt_executable.Size = new System.Drawing.Size(453, 20);
            this.txt_executable.TabIndex = 1;
            this.txt_executable.TextChanged += new System.EventHandler(this.txt_executable_TextChanged);
            // 
            // lbl_app_executable
            // 
            this.lbl_app_executable.AutoSize = true;
            this.lbl_app_executable.Location = new System.Drawing.Point(3, 6);
            this.lbl_app_executable.Name = "lbl_app_executable";
            this.lbl_app_executable.Size = new System.Drawing.Size(101, 13);
            this.lbl_app_executable.TabIndex = 0;
            this.lbl_app_executable.Text = global::HeliosDisplayManagement.Resources.Language.Executable_Address;
            // 
            // txt_process
            // 
            this.txt_process.Enabled = false;
            this.txt_process.Location = new System.Drawing.Point(114, 29);
            this.txt_process.Name = "txt_process";
            this.txt_process.Size = new System.Drawing.Size(356, 20);
            this.txt_process.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(476, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = global::HeliosDisplayManagement.Resources.Language.Timeout;
            // 
            // cb_process
            // 
            this.cb_process.AutoSize = true;
            this.cb_process.Location = new System.Drawing.Point(6, 31);
            this.cb_process.Name = "cb_process";
            this.cb_process.Size = new System.Drawing.Size(103, 17);
            this.cb_process.TabIndex = 3;
            this.cb_process.Text = global::HeliosDisplayManagement.Resources.Language.Waiting_Process;
            this.cb_process.UseVisualStyleBackColor = true;
            this.cb_process.CheckedChanged += new System.EventHandler(this.Controls_CheckedChanged);
            // 
            // nud_timeout
            // 
            this.nud_timeout.Enabled = false;
            this.nud_timeout.Location = new System.Drawing.Point(527, 29);
            this.nud_timeout.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.nud_timeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nud_timeout.Name = "nud_timeout";
            this.nud_timeout.Size = new System.Drawing.Size(70, 20);
            this.nud_timeout.TabIndex = 6;
            this.nud_timeout.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // rb_steam
            // 
            this.rb_steam.AutoSize = true;
            this.rb_steam.Location = new System.Drawing.Point(9, 100);
            this.rb_steam.Name = "rb_steam";
            this.rb_steam.Size = new System.Drawing.Size(86, 17);
            this.rb_steam.TabIndex = 2;
            this.rb_steam.Text = global::HeliosDisplayManagement.Resources.Language.Steam_Game;
            this.rb_steam.UseVisualStyleBackColor = true;
            this.rb_steam.CheckedChanged += new System.EventHandler(this.Controls_CheckedChanged);
            // 
            // rb_standalone
            // 
            this.rb_standalone.AutoSize = true;
            this.rb_standalone.Checked = true;
            this.rb_standalone.Location = new System.Drawing.Point(9, 19);
            this.rb_standalone.Name = "rb_standalone";
            this.rb_standalone.Size = new System.Drawing.Size(120, 17);
            this.rb_standalone.TabIndex = 0;
            this.rb_standalone.TabStop = true;
            this.rb_standalone.Text = global::HeliosDisplayManagement.Resources.Language.Standalone_Process;
            this.rb_standalone.UseVisualStyleBackColor = true;
            this.rb_standalone.CheckedChanged += new System.EventHandler(this.Controls_CheckedChanged);
            // 
            // txt_args
            // 
            this.txt_args.Enabled = false;
            this.txt_args.Location = new System.Drawing.Point(113, 155);
            this.txt_args.Name = "txt_args";
            this.txt_args.Size = new System.Drawing.Size(513, 20);
            this.txt_args.TabIndex = 5;
            // 
            // cb_args
            // 
            this.cb_args.AutoSize = true;
            this.cb_args.Location = new System.Drawing.Point(9, 157);
            this.cb_args.Name = "cb_args";
            this.cb_args.Size = new System.Drawing.Size(76, 17);
            this.cb_args.TabIndex = 4;
            this.cb_args.Text = global::HeliosDisplayManagement.Resources.Language.Arguments;
            this.cb_args.UseVisualStyleBackColor = true;
            this.cb_args.CheckedChanged += new System.EventHandler(this.Controls_CheckedChanged);
            // 
            // cb_temp
            // 
            this.cb_temp.AutoSize = true;
            this.cb_temp.Location = new System.Drawing.Point(15, 309);
            this.cb_temp.Name = "cb_temp";
            this.cb_temp.Size = new System.Drawing.Size(226, 17);
            this.cb_temp.TabIndex = 3;
            this.cb_temp.Text = global::HeliosDisplayManagement.Resources.Language.Temporarily_switch_with_process_monitoring;
            this.cb_temp.UseVisualStyleBackColor = true;
            this.cb_temp.CheckedChanged += new System.EventHandler(this.Controls_CheckedChanged);
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dv_profile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(184)))), ((int)(((byte)(196)))));
            this.dv_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(-1, -1);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(658, 304);
            this.dv_profile.TabIndex = 2;
            // 
            // dialog_open
            // 
            this.dialog_open.DefaultExt = "exe";
            this.dialog_open.FileName = "*.exe";
            this.dialog_open.Filter = global::HeliosDisplayManagement.Resources.Language.Executable_Files_Filter;
            this.dialog_open.RestoreDirectory = true;
            this.dialog_open.SupportMultiDottedExtensions = true;
            // 
            // dialog_save
            // 
            this.dialog_save.DefaultExt = "lnk";
            this.dialog_save.DereferenceLinks = false;
            this.dialog_save.Filter = global::HeliosDisplayManagement.Resources.Language.Shortcuts_Filter;
            this.dialog_save.RestoreDirectory = true;
            // 
            // ShortcutForm
            // 
            this.AcceptButton = this.btn_save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(656, 557);
            this.Controls.Add(this.cb_temp);
            this.Controls.Add(this.g_temp);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.dv_profile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShortcutForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::HeliosDisplayManagement.Resources.Language.Create_Helios_Shortcut;
            this.g_temp.ResumeLayout(false);
            this.g_temp.PerformLayout();
            this.p_steam.ResumeLayout(false);
            this.p_steam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_steamtimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_steamappid)).EndInit();
            this.p_standalone.ResumeLayout(false);
            this.p_standalone.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_timeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.GroupBox g_temp;
        private System.Windows.Forms.CheckBox cb_temp;
        private System.Windows.Forms.RadioButton rb_standalone;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_args;
        private System.Windows.Forms.CheckBox cb_args;
        private System.Windows.Forms.NumericUpDown nud_timeout;
        private System.Windows.Forms.Panel p_standalone;
        private System.Windows.Forms.Button btn_app_executable;
        private System.Windows.Forms.TextBox txt_executable;
        private System.Windows.Forms.Label lbl_app_executable;
        private System.Windows.Forms.TextBox txt_process;
        private System.Windows.Forms.CheckBox cb_process;
        private System.Windows.Forms.RadioButton rb_steam;
        private System.Windows.Forms.Panel p_steam;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_steamappid;
        private System.Windows.Forms.Label lbl_steamname;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nud_steamtimeout;
        private System.Windows.Forms.Button nud_steamapps;
        private System.Windows.Forms.OpenFileDialog dialog_open;
        private System.Windows.Forms.SaveFileDialog dialog_save;
    }
}