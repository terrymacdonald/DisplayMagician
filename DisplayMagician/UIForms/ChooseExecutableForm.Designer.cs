
namespace DisplayMagician.UIForms
{
    partial class ChooseExecutableForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseExecutableForm));
            btn_select_app = new System.Windows.Forms.Button();
            dialog_open = new System.Windows.Forms.OpenFileDialog();
            lbl_installed_apps = new System.Windows.Forms.Label();
            lbl_title = new System.Windows.Forms.Label();
            lbl_select_exe = new System.Windows.Forms.Label();
            gb_select_exe = new System.Windows.Forms.GroupBox();
            btn_select_exe = new System.Windows.Forms.Button();
            lbl_or = new System.Windows.Forms.Label();
            ilv_installed_apps = new Manina.Windows.Forms.ImageListView();
            btn_back = new System.Windows.Forms.Button();
            gb_select_exe.SuspendLayout();
            SuspendLayout();
            // 
            // btn_select_app
            // 
            btn_select_app.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_select_app.Enabled = false;
            btn_select_app.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_select_app.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_select_app.ForeColor = System.Drawing.Color.White;
            btn_select_app.Location = new System.Drawing.Point(196, 584);
            btn_select_app.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_select_app.Name = "btn_select_app";
            btn_select_app.Size = new System.Drawing.Size(411, 37);
            btn_select_app.TabIndex = 40;
            btn_select_app.Text = "Select Application from List";
            btn_select_app.UseVisualStyleBackColor = true;
            btn_select_app.Click += btn_select_app_Click;
            // 
            // dialog_open
            // 
            dialog_open.FileName = "openFileDialog1";
            // 
            // lbl_installed_apps
            // 
            lbl_installed_apps.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_installed_apps.AutoSize = true;
            lbl_installed_apps.BackColor = System.Drawing.Color.Black;
            lbl_installed_apps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_installed_apps.ForeColor = System.Drawing.Color.White;
            lbl_installed_apps.Location = new System.Drawing.Point(164, 97);
            lbl_installed_apps.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_installed_apps.Name = "lbl_installed_apps";
            lbl_installed_apps.Size = new System.Drawing.Size(406, 20);
            lbl_installed_apps.TabIndex = 2;
            lbl_installed_apps.Text = "Select the Application you wish to use from the list below";
            lbl_installed_apps.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_title
            // 
            lbl_title.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_title.AutoSize = true;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(531, 22);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(375, 33);
            lbl_title.TabIndex = 44;
            lbl_title.Text = "Select an Executable to use";
            // 
            // lbl_select_exe
            // 
            lbl_select_exe.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_select_exe.AutoSize = true;
            lbl_select_exe.BackColor = System.Drawing.Color.Black;
            lbl_select_exe.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_select_exe.ForeColor = System.Drawing.Color.White;
            lbl_select_exe.Location = new System.Drawing.Point(904, 97);
            lbl_select_exe.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_select_exe.Name = "lbl_select_exe";
            lbl_select_exe.Size = new System.Drawing.Size(438, 20);
            lbl_select_exe.TabIndex = 45;
            lbl_select_exe.Text = "Or, choose an executable from somewhere on your computer";
            lbl_select_exe.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // gb_select_exe
            // 
            gb_select_exe.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            gb_select_exe.Controls.Add(btn_select_exe);
            gb_select_exe.Location = new System.Drawing.Point(863, 132);
            gb_select_exe.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_select_exe.Name = "gb_select_exe";
            gb_select_exe.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gb_select_exe.Size = new System.Drawing.Size(598, 440);
            gb_select_exe.TabIndex = 47;
            gb_select_exe.TabStop = false;
            // 
            // btn_select_exe
            // 
            btn_select_exe.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_select_exe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_select_exe.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_select_exe.ForeColor = System.Drawing.Color.White;
            btn_select_exe.Location = new System.Drawing.Point(85, 201);
            btn_select_exe.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_select_exe.Name = "btn_select_exe";
            btn_select_exe.Size = new System.Drawing.Size(440, 36);
            btn_select_exe.TabIndex = 41;
            btn_select_exe.Text = "Choose Exe from your Computer";
            btn_select_exe.UseVisualStyleBackColor = true;
            btn_select_exe.Click += btn_select_exe_Click;
            // 
            // lbl_or
            // 
            lbl_or.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_or.AutoSize = true;
            lbl_or.BackColor = System.Drawing.Color.Black;
            lbl_or.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_or.ForeColor = System.Drawing.Color.White;
            lbl_or.Location = new System.Drawing.Point(786, 337);
            lbl_or.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_or.Name = "lbl_or";
            lbl_or.Size = new System.Drawing.Size(51, 29);
            lbl_or.TabIndex = 48;
            lbl_or.Text = "OR";
            lbl_or.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ilv_installed_apps
            // 
            ilv_installed_apps.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ilv_installed_apps.Location = new System.Drawing.Point(31, 138);
            ilv_installed_apps.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_installed_apps.Name = "ilv_installed_apps";
            ilv_installed_apps.PersistentCacheDirectory = "";
            ilv_installed_apps.PersistentCacheSize = 100L;
            ilv_installed_apps.Size = new System.Drawing.Size(740, 433);
            ilv_installed_apps.TabIndex = 49;
            ilv_installed_apps.UseWIC = true;
            ilv_installed_apps.ItemClick += ilv_installed_apps_ItemClick;
            ilv_installed_apps.VisibleChanged += ilv_installed_apps_VisibleChanged;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1396, 616);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 50;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            // 
            // ChooseExecutableForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1498, 657);
            Controls.Add(btn_back);
            Controls.Add(ilv_installed_apps);
            Controls.Add(lbl_or);
            Controls.Add(gb_select_exe);
            Controls.Add(lbl_select_exe);
            Controls.Add(lbl_title);
            Controls.Add(btn_select_app);
            Controls.Add(lbl_installed_apps);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChooseExecutableForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Choose an application";
            TopMost = true;
            Activated += ChooseExecutableForm_Activated;
            Load += ChooseExecutableForm_Load;
            Shown += ChooseExecutableForm_Shown;
            gb_select_exe.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btn_select_app;
        private System.Windows.Forms.OpenFileDialog dialog_open;
        private System.Windows.Forms.Label lbl_installed_apps;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_select_exe;
        private System.Windows.Forms.GroupBox gb_select_exe;
        private System.Windows.Forms.Label lbl_or;
        private Manina.Windows.Forms.ImageListView ilv_installed_apps;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_select_exe;
    }
}